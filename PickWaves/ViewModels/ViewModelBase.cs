namespace DcmsMobile.PickWaves.ViewModels
{
    public abstract class ViewModelBase
    {
        /// <summary>
        /// True if the logged in user has the rights to manage all aspects of a pick wave
        /// </summary>
        public bool UserIsManager { get; set; }

        /// <summary>
        /// The role needed to work on Pick waves.
        /// </summary>
        public string ManagerRoleName { get; set; }

    }
}