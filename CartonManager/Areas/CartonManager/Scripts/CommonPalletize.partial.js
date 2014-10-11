/// <reference path="../../../Scripts/jquery-1.6.2-vsdoc.js" />
/*
  This script is used by UI PalletizeUI, MarkReworkComplete and AbandonRework and Locating UI.
  You must have a text box with id tbScan where cartons or pallets will be scanned.
  You can also have a button with id btnGo which will behave the same as pressing enter on tbScan.
*/
$(document).ready(function () {
    $('#tbScan').handlescan();
    $('#btnGo').click(function (e) {
        $('#tbScan').handlescan('scan');
        $('#tbScan').focus();
    }).button();
});

//$Id$
