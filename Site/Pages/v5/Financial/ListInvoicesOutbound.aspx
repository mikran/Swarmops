﻿<%@ Page Title="" Language="C#" MasterPageFile="~/Master-v5.master" AutoEventWireup="true" Inherits="Swarmops.Frontend.Pages.v5.Financial.ListInvoicesOutbound" Codebehind="ListInvoicesOutbound.aspx.cs" %>
<%@ Register src="~/Controls/v5/Base/ModalDialog.ascx" tagname="ModalDialog" tagprefix="Swarmops5" %>
<%@ Register src="~/Controls/v5/Financial/ComboBudgets.ascx" tagname="ComboBudgets" tagprefix="Swarmops5" %>
<%@ Register src="~/Controls/v5/Financial/CurrencyTextBox.ascx" tagname="CurrencyTextBox" tagprefix="Swarmops5" %>

<asp:Content ID="Content1" ContentPlaceHolderID="PlaceHolderHead" Runat="Server">
	<script type="text/javascript" src="/Scripts/fancybox/jquery.fancybox-1.3.4.js"></script>
    <script type="text/javascript" src="/Scripts/fancybox/jquery.mousewheel-3.0.4.pack.js"></script>
	<link rel="stylesheet" type="text/css" href="/Scripts/fancybox/jquery.fancybox-1.3.4.css" media="screen" />

    
    <script type="text/javascript">
        function preload(arrayOfImages) {
            $(arrayOfImages).each(function () {
                (new Image()).src = this;
            });
        }

        preload([
            '/Images/Abstract/ajaxloader-medium.gif',
            '/Images/Abstract/ajaxloader-48x36px.gif',
            '/Images/Icons/iconshock-balloon-yes-128x96px-hot.png',
            '/Images/Icons/iconshock-balloon-yes-128x96px-disabled.png',
            '/Images/Icons/iconshock-balloon-yes-128x96px-hot-disabled.png',
            '/Images/Icons/iconshock-balloon-yes-128x96px-gold.png',
            '/Images/Icons/iconshock-balloon-yes-128x96px-hot-gold.png',
            '/Images/Icons/iconshock-balloon-no-128x96px-hot.png',
            '/Images/Icons/iconshock-green-tick-128x96px.png',
            '/Images/Icons/iconshock-red-cross-128x96px.png',
            '/Images/Icons/iconshock-red-cross-circled-128x96px.png',
            '/Images/Icons/iconshock-balloon-undo-128x96px.png',
            '/Images/Icons/iconshock-balloon-undo-128x96px-hot.png'
        ]);

        /* -- commented out -- do we need attestation logic for this page?
        loadUninitializedBudgets(); // no need to wait for doc.ready to load operating params

        SwarmopsJS.ajaxCall("/Pages/v5/Financial/AttestCosts.aspx/GetRemainingBudgets", {}, function(data) {
            data.forEach(function(accountData, dummy1, dummy2) {
                budgetRemainingLookup[accountData.AccountId] = accountData.Remaining;
            });

            if (budgetRemainingLookup.rowsLoaded == true) {
                setAttestability();
            }

            budgetRemainingLookup.budgetsLoaded = true;
        });*/

        // Doc.ready:

        $(document).ready(function() {

            // we're still in document.ready

            $('input:radio[name="ModalOptions"]').change(function () {
                var pickedButtonName = $(this).val();
                $('div.radioOption').slideUp();
                $('div#radioOption' + pickedButtonName).slideDown();
            });
        });


        var recordId = '';
        var accountId = 0;
        var budgetRemainingLookup = {};
        var budgetUninitializedLookup = {};
        var uninitializedPopupDisplayed = false;


    </script>
    
     <style type="text/css">
        .datagrid-row-selected,.datagrid-row-over{
            background:transparent;
        }
        .rowPrevious {
            color: #AAA;
        }

        div.radioOption {
            margin-top: -20px;
            padding-bottom: 10px;
            padding-left: 12px;
            margin-right: 10px;
        }
        body.ltr div.radioOption {
            padding-left: initial;
            margin-right: initial;
            padding-right: 12px;
            margin-left: 10px;
        }

    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="PlaceHolderMain" Runat="Server">
    <h2><asp:Label runat="server" ID="LabelListInboundInvoicesHeader" Text="XYZ List Inbound Invoices" /></h2>
    <table id="TableInboundInvoices" class="easyui-datagrid" style="width:680px;height:400px"
        data-options="rownumbers:false,singleSelect:false,fit:false,fitWidth:true,loading:false,selectOnCheck:true,checkOnSelect:true,url:'Json-ListInvoicesOutbound.aspx'"
        idField="itemId">
        <thead>
            <tr>  
                <th data-options="field:'item',width:60"><asp:Label ID="LabelGridHeaderItem" runat="server" Text="XYZ Item"/></th>  
                <th data-options="field:'dueDate',width:60"><asp:Label ID="LabelGridHeaderDueDate" runat="server" Text="XYZ DueDate" /></th>  
                <th data-options="field:'sender',width:140,sortable:true"><asp:Label ID="LabelGridHeaderBeneficiary" runat="server" Text="XYZ Beneficiary" /></th>  
                <th data-options="field:'budget',width:140,sortable:true"><asp:Label ID="LabelGridHeaderBudget" runat="server" Text="XYZ Budget" /></th>  
                <th data-options="field:'amount',width:100,align:'right',sortable:true,order:'asc'"><asp:Label ID="LabelGridHeaderRequested" runat="server" Text="XYZ Requested" /></th>
                <th data-options="field:'progress',width:76,align:'center'"><asp:Label ID="LabelGridHeaderProgress" runat="server" Text="XYZ Progress" /></th>
                <th data-options="field:'dox',width:40,align:'center'"><asp:Label ID="LabelGridHeaderDocs" runat="server" Text="Doxyz" /></th>
                <th data-options="field:'actions',width:53,align:'center'"><asp:Label ID="LabelGridHeaderActions" runat="server" Text="XYZAction" /></th>
            </tr>  
        </thead>
    </table>
    
    <div style="display:none">
    <!-- a href links for FancyBox to trigger on -->
    
    <asp:Repeater runat="server" ID="RepeaterLightboxItems">
        <ItemTemplate>
            <a href="/Pages/v5/Support/StreamUpload.aspx?DocId=<%# Eval("DocId") %>&hq=1" title="<%# Eval("Title") %>" class="FancyBox_Gallery" rel="<%# Eval("BaseId") %>">&nbsp;</a>
        </ItemTemplate>
    </asp:Repeater>

    </div>  
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="PlaceHolderSide" Runat="Server">
</asp:Content>

