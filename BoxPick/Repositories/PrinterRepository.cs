using System;
using EclipseLibrary.Oracle;

namespace DcmsMobile.BoxPick.Repositories
{
    public partial class BoxPickRepository
    {
        /// <summary>
        /// Prints the pallet passed
        /// We pass 'ACOPIES' parameter as null in 'PKG_BOXEXPEDITE.PRINT_PALLET' so that default documents are printed.
        /// </summary>
        /// <param name="palletId"></param>
        /// <param name="printer"></param>
        public void Print(string palletId, string printer)
        {
            if (string.IsNullOrEmpty(palletId))
            {
                throw new ArgumentNullException("Pallet is null");
            }

            if (string.IsNullOrEmpty(printer))
            {
                throw new ArgumentNullException("Printer is null");
            }

            const string QUERY = @"
                                BEGIN
                                  <proxy />PKG_BOXEXPEDITE.PRINT_PALLET(APALLET_ID    => :APALLET_ID,
                                                                        APRINTER_NAME => :APRINTER_NAME,
                                                                        ACOPIES       => NULL);
                                END;
            ";
            var binder = SqlBinder.Create()
                                 .Parameter("APALLET_ID", palletId)
                                 .Parameter("APRINTER_NAME", printer);
            _db.ExecuteNonQuery(QUERY, binder);
        }

        /// <summary>
        /// Fetches the Printer available for the pallet id
        /// </summary>
        /// <param name="palletId"> </param>
        public string GetPrinter(string palletId)
        {
            if (string.IsNullOrEmpty(palletId))
            {
                throw new ArgumentNullException("Pallet is null");
            }

            var printer = string.Empty;
            const string QUERY = @"
                begin
                :result := <proxy />pkg_pul.get_printer(apallet_id => :apallet_id);
                end;
            ";
            var binder = SqlBinder.Create()
                .Parameter("apallet_id", palletId)
                .OutParameter("result", val => printer = val);

            _db.ExecuteNonQuery(QUERY, binder);
            return printer;
        }
    }
}



//$Id$