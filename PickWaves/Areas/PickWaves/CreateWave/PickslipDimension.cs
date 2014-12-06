using System.ComponentModel.DataAnnotations;

namespace DcmsMobile.PickWaves.Areas.PickWaves.CreateWave
{
    /// <summary>
    /// The column to use as dimension while retrieving pickslips. 
    /// </summary>
    /// <remarks>
    /// ShortName is used for generating bucket name
    /// </remarks>
    public enum PickslipDimension
    {
        /// <summary>
        /// Value Type: string
        /// </summary>
        [Display(Name = "Priority", ShortName="Pr")]
        Priority,

        /// <summary>
        /// Value Type: string
        /// </summary>
        [Display(Name = "Customer Store", ShortName="Store")]
        CustomerStore,

        /// <summary>
        /// Value Type: string
        /// </summary>
        [Display(Name = "Label", ShortName="Lbl")]
        Label,

        /// <summary>
        /// Value Type: Date
        /// </summary>
        [Display(Name = "Import Date", ShortName="Imp")]
        [DataType(DataType.Date)]
        ImportDate,

        /// <summary>
        /// Value Type: Date
        /// </summary>
        [Display(Name = "Start Date", ShortName="Start")]
        [DataType(DataType.Date)]
        StartDate,

        /// <summary>
        /// Value Type: Date
        /// </summary>
        [Display(Name = "Cancel Date", ShortName="Cancel")]
        [DataType(DataType.Date)]
        CancelDate,

        /// <summary>
        /// Value Type: string
        /// </summary>
        [Display(Name = "Customer Order Type", ShortName="Typ")]    
        CustomerOrderType,

        /// <summary>
        /// Value Type: string
        /// </summary>
        [Display(Name = "Sale Type", ShortName="Sale")]    
        SaleTypeId,

        /// <summary>
        /// Value Type: string
        /// </summary>
        [Display(Name = "Purchase Order", ShortName="PO")]    
        PurchaseOrder,

        /// <summary>
        /// Value Type: Date
        /// </summary>
        [Display(Name = "DC Cancel Date", ShortName="DCCan")]
        [DataType(DataType.Date)]
        CustomerDcCancelDate,

        [Display(Name = "Customer DC", ShortName="DC")]
        CustomerDc
    }
}