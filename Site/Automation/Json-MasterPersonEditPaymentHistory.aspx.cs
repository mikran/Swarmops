﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Swarmops.Common;
using Swarmops.Common.Enums;
using Swarmops.Logic.Financial;
using Swarmops.Logic.Swarm;

namespace Swarmops.Frontend.Automation
{
    public partial class Json_MasterPersonEditPaymentHistory : DataV5Base
    {
        private AuthenticationData _authenticationData;
        private Person _person;
        private Dictionary<int, Payout> _payoutLookup;
        private Dictionary<int, string> _payoutDescriptionOverride; 

        protected void Page_Load(object sender, EventArgs e)
        {
            this._authenticationData = GetAuthenticationDataAndCulture();

            int personId = Int32.Parse(Request.QueryString["PersonId"]); // may throw and that's okay, returning a 500 instead of Json the caller shouldn't see
            if (personId == 0)
            {
                personId = _authenticationData.CurrentUser.Identity;
            }

            this._person = Person.FromIdentity(personId);
            if (this._person.Identity != _authenticationData.CurrentUser.Identity)
            {
                if (!_authenticationData.Authority.CanSeePerson(_person))
                {
                    throw new UnauthorizedAccessException();
                }
            }

            List<PaymentHistoryLineItem> list = new List<PaymentHistoryLineItem>();
            _payoutLookup = new Dictionary<int, Payout>();
            _payoutDescriptionOverride = new Dictionary<int, string>();

            list.AddRange(GetAmountsOwed());
            list.AddRange(GetAmountsPaid());
            //list.SortByDate();

            Response.ContentType = "application/json";
            Response.Output.WriteLine("{\"rows\": " + JsonWriteItems(list) + ", \"footer\": [" +
                                       JsonWriteFooter(list) + "]}");

            Response.End();

        }


        public List<PaymentHistoryLineItem> GetAmountsOwed()
        {
            List<PaymentHistoryLineItem> items = new List<PaymentHistoryLineItem>();

            // Expense claims

            ExpenseClaims expenses = ExpenseClaims.FromClaimingPersonAndOrganization(_person,
                _authenticationData.CurrentOrganization);

            foreach (ExpenseClaim claim in expenses)
            {
                if (claim.Open || claim.PaidOut) // if both these are false, the claim was denied and shouldn't be listed
                {
                    PaymentHistoryLineItem newItem = new PaymentHistoryLineItem();
                    newItem.Id = "E" + claim.Identity.ToString(CultureInfo.InvariantCulture);
                    newItem.Name = Resources.Global.Financial_ExpenseClaim + " #" + claim.Identity.ToString("N0");
                    newItem.Description = claim.Description;
                    newItem.OpenedDate = claim.CreatedDateTime;
                    newItem.OwedToPerson = claim.AmountCents;

                    Payout payout = claim.Payout;
                    if (payout != null && payout.Open == false)
                    {
                        _payoutLookup[payout.Identity] = payout;
                        newItem.ClosedDate = payout.FinancialTransaction.DateTime;
                    }

                    items.Add(newItem);
                }
            }

            // Salaries

            Salaries salaries = Salaries.ForPersonAndOrganization(_person, _authenticationData.CurrentOrganization, true);

            foreach (Salary salary in salaries)
            {
                if (salary.Open || salary.NetPaid) // either of these must be open for the salary to be valid
                {
                    PaymentHistoryLineItem newItem = new PaymentHistoryLineItem();
                    newItem.Id = "S" + salary.Identity.ToString(CultureInfo.InvariantCulture);
                    newItem.Name = Resources.Global.Financial_Salary;
                    newItem.Description = String.Format(Resources.Global.Financial_SalaryDualSpecification, salary.Identity, salary.PayoutDate);
                    newItem.OwedToPerson = salary.NetSalaryCents;

                    FinancialTransaction openTx = FinancialTransaction.FromDependency(salary);
                    if (openTx != null)
                    {
                        newItem.OpenedDate = openTx.DateTime;
                    }

                    Payout payout = Payout.FromDependency(salary, FinancialDependencyType.Salary);
                    if (payout != null && payout.Open == false)
                    {
                        _payoutLookup[payout.Identity] = payout;
                        newItem.ClosedDate = payout.FinancialTransaction.DateTime;
                    }

                    items.Add(newItem);
                }
            }

            // Cash advances

            CashAdvances advances = CashAdvances.ForPersonAndOrganization(_person,
                _authenticationData.CurrentOrganization, true);

            foreach (CashAdvance advance in advances)
            {
                if (advance.Open || advance.PaidOut)
                {
                    Payout payout = advance.PayoutOut;
                    if (payout != null)
                    {
                        _payoutLookup[payout.Identity] = payout;
                        _payoutDescriptionOverride[payout.Identity] =
                            String.Format(Resources.Global.Financial_CashAdvanceSpecification, advance.Identity.ToString("N0"));
                    }
                }
            }

            return items;
        }

        public List<PaymentHistoryLineItem> GetAmountsPaid()
        {
            List<PaymentHistoryLineItem> items = new List<PaymentHistoryLineItem>();

            foreach (Payout payout in _payoutLookup.Values)
            {
                if (payout.Open)
                {
                    continue; // do not list unconfirmed payouts
                }

                PaymentHistoryLineItem newItem = new PaymentHistoryLineItem();
                newItem.Id = "PO" + payout.Identity.ToString(CultureInfo.InvariantCulture);
                if (_payoutDescriptionOverride.ContainsKey(payout.Identity))
                {
                    newItem.Name = _payoutDescriptionOverride[payout.Identity];
                }
                else
                {
                    newItem.Name = String.Format(Resources.Global.Financial_PayoutSpecification, payout.Identity);
                }
                newItem.Description = String.Empty;
                newItem.PaidToPerson = payout.AmountCents;

                FinancialTransaction closeTx = payout.FinancialTransaction;
                if (closeTx != null) // it damn well should not be null since Open is false
                {
                    newItem.ClosedDate = closeTx.DateTime;
                }

                items.Add(newItem);
            }

            return items;
        }

        public string JsonWriteItems(List<PaymentHistoryLineItem> items)
        {
            StringBuilder result = new StringBuilder(16384);
            result.Append("[");

            foreach (PaymentHistoryLineItem item in items)
            { 
                result.Append("{");
                result.AppendFormat(
                    "\"id\":\"{0}\",\"name\":\"{1}\",\"description\":\"{2}\",\"opened\":\"{3}\",\"owedToPerson\":\"{4}\",\"paidToPerson\":\"{5}\",\"closed\":\"{6}\"",
                    JsonSanitize(item.Id),
                    JsonSanitize(item.Name),
                    JsonSanitize(item.Description),
                    item.OpenedDate > Constants.DateTimeLowThreshold? item.OpenedDate.ToString("yyyy-MMM-dd") : string.Empty,
                    item.OwedToPerson > 0 ? (item.OwedToPerson/100.0).ToString("N2") : string.Empty,
                    string.Empty,
                    item.ClosedDate < Constants.DateTimeHighThreshold? item.ClosedDate.ToString("yyyy-MMM-dd"): string.Empty

                );

                result.Append("},");
            }

            result.Remove(result.Length - 1, 1); // remove last comma
            result.Append("]");

            return result.ToString();
        }

public string JsonWriteFooter(List<PaymentHistoryLineItem> items)
        {
            return string.Empty;
        }

        public class PaymentHistoryLineItem
        {
            public PaymentHistoryLineItem()
            {
                this.OpenedDate = Constants.DateTimeLow;
                this.ClosedDate = Constants.DateTimeHigh;
            }

            public string Id { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public Int64 OwedToPerson { get; set; }
            public Int64 PaidToPerson { get; set; }

            public DateTime OpenedDate { get; set; }
            public DateTime ClosedDate { get; set; }
        }

    }
}