namespace Nop.Core.Domain.Messages
{
    /// <summary>
    /// Represents message template system names
    /// </summary>
    public static partial class MessageTemplateSystemNames
    {
        #region Customer

        /// <summary>
        /// Represents system name of notification about new registration
        /// </summary>
        public const string CustomerRegisteredNotification = "NewCustomer.Notification";

        /// <summary>
        /// Represents system name of customer welcome message
        /// </summary>
        public const string CustomerWelcomeMessage = "Customer.WelcomeMessage";

        /// <summary>
        /// Represents system name of email validation message
        /// </summary>
        public const string CustomerEmailValidationMessage = "Customer.EmailValidationMessage";

        /// <summary>
        /// Represents system name of email revalidation message
        /// </summary>
        public const string CustomerEmailRevalidationMessage = "Customer.EmailRevalidationMessage";

        /// <summary>
        /// Represents system name of password recovery message
        /// </summary>
        public const string CustomerPasswordRecoveryMessage = "Customer.PasswordRecovery";

        #endregion

        #region InformationRepository

        /// <summary>
        /// 
        /// </summary>
        public const string RI_UPDATE_VERSION = "RepositorioInformacion.UpdateVersion";
        public const string RI_DELETE_FILE = "RepositorioInformacion.DeleteFile";
        public const string RI_PUBLISHED_VERSION = "RepositorioInformacion.PublishedVersion";
        public const string RI_AUTHORIZED_VERSION = "RepositorioInformacion.AuthorizedVersion";
        public const string RI_REJECTED_VERSION = "RepositorioInformacion.RejectedVersion";

        #endregion
    }
}