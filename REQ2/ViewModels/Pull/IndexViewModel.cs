using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.UI;

namespace DcmsMobile.REQ2.ViewModels.Pull
{
    public class IndexViewModel
    {
        /// <summary>
        /// List of areas 
        /// </summary>
        [ReadOnly(true)]
        public IList<AreaModel> AreaList { get; set; }

        /// <summary>
        /// The user choice is posted here
        /// </summary>
        [Required]
        [ReadOnly(false)]
        [Range(minimum: 1, maximum: int.MaxValue, ErrorMessage = "This is not a valid choice.")]
        public int? Choice { get; set; }

        /// <summary>
        /// This will be posted back
        /// </summary>
        [ReadOnly(false)]
        public string SerializedAreaList
        {
            get
            {
                if (AreaList == null)
                {
                    return null;
                }
                var formatter = new ObjectStateFormatter();

                // Call the Serialize method to serialize the ArrayList to a Base64 encoded string. 
                // Use pair array to optimize the size of the view state
                var query = (from area in AreaList
                             select new Pair(area.SourceAreaId, area.DestinationAreaId)).ToArray();
                string base64StateString = formatter.Serialize(query);
                return base64StateString;
            }
            set
            {
                var formatter = new ObjectStateFormatter();
                var pairs = (Pair[])formatter.Deserialize(value);
                this.AreaList = (from pair in pairs
                                 select new AreaModel
                                 {
                                     SourceAreaId = (string)pair.First,
                                     DestinationAreaId = (string)pair.Second
                                 }).ToArray();
            }
        }
    }

    public class AreaModel
    {
        public string SourceBuildingId { get; set; }

        public string DestinationAreaId { get; set; }

        public string SourceAreaId { get; set; }

        public string DestinationBuildingId { get; set; }

        public int PullableCartonCount { get; set; }

        public string SourceShortName { get; set; }

        public string TopRequestId { get; set; }

        public string DestinationShortName
        {
            get;
            set;
        }

        [DisplayFormat(HtmlEncode = false)]
        public string DisplayName
        {
            get
            {
                if (SourceBuildingId == DestinationBuildingId)
                {
                    return string.Format("{0}: <strong>{1}</strong> &rarr; {2} <em>{3:N0} cartons<em>: Top request {4}", SourceBuildingId,
                        SourceShortName, DestinationShortName, PullableCartonCount,TopRequestId);
                }
                else
                {
                    return string.Format("<strong>{0}:{1}</strong> &rarr; {2}:{3} <em>{4:N0} cartons</em>: Top request {5}", SourceBuildingId,
                        SourceShortName, DestinationBuildingId, DestinationShortName, PullableCartonCount,TopRequestId);
                }
            }
        }
    }
}