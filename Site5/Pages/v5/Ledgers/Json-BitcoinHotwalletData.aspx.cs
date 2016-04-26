﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Resources;
using Swarmops.Logic.Financial;
using Swarmops.Logic.Security;

namespace Swarmops.Frontend.Pages.v5.Ledgers
{
    public partial class Json_BitcoinHotwalletData : DataV5Base
    {
        private AuthenticationData _authenticationData;

        protected void Page_Load(object sender, EventArgs e)
        {
            PageAccessRequired = new Access(CurrentOrganization, AccessAspect.BookkeepingDetails, AccessType.Read);
            this._authenticationData = GetAuthenticationDataAndCulture();

            HotBitcoinAddresses addresses = HotBitcoinAddresses.ForOrganization (_authenticationData.CurrentOrganization);

            Response.ContentType = "application/json";
            Response.Output.WriteLine(FormatJson(addresses));
            Response.End();
        }



        private string FormatJson(HotBitcoinAddresses addresses)
        {
            StringBuilder result = new StringBuilder(16384);

            double conversionRate = 1.0;
            if (!this._authenticationData.CurrentOrganization.Currency.IsBitcoin)
            {
                long fiatCentsPerCoin = new Money(100000000, Currency.Bitcoin).ToCurrency (_authenticationData.CurrentOrganization.Currency).Cents;
                conversionRate = fiatCentsPerCoin/100000000.0; // on satoshi level
            }

            result.Append("{\"rows\":[");

            Int64 satoshisTotal = 0;

            foreach (HotBitcoinAddress address in addresses)
            {
                result.Append("{");
                result.AppendFormat (
                    "\"id\":\"{0}\"," +
                    "\"derivePath\":\"{1}\"," +
                    "\"address\":\"{2}\"," +
                    "\"balanceMicrocoins\":\"{3}\"," +
                    "\"balanceFiat\":\"{4}\"",
                    address.Identity,
                    address.DerivationPath,
                    address.Address,
                    (address.BalanceSatoshis/100.0).ToString ("N2"),
                    (address.BalanceSatoshis/100.0*conversionRate).ToString ("N2")
                );
                result.Append("},");
                satoshisTotal += address.BalanceSatoshis;
            }

            if (addresses.Count > 0)
            {
                result.Remove(result.Length - 1, 1); // remove last comma
            }

            result.Append("],\"footer\":[");

            result.Append("{");

            result.AppendFormat("\"derivePath\":\"TOTAL\",\"balanceMicrocoins\":\"{0}\",\"balanceFiat\":\"{1:N2}\"",
                (satoshisTotal / 100.0).ToString("N2"), (satoshisTotal / 100.0 * conversionRate).ToString("N2"));

            result.Append("}]}"); // on separate line to suppress warning

            return result.ToString();
        }



    }
}