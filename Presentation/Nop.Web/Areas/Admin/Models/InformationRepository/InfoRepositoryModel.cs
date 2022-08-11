using System;
using System.Collections.Generic;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Web.Areas.Admin.Models.InformationRepository
{
    /// <summary>
    /// Represents a customer role search model
    /// </summary>
    public partial record InfoRepositoryModel : BaseSearchModel
    {

        #region Ctor

        public InfoRepositoryModel()
        {
            UploadFileModel = new FileInfoModel();
            InfoFolder = new InfoRepositoryFolderModel();
            DirectoryListModel = new List<DirectoriesListModel>();
            FileListModel = new List<FileInfoModel>();
            FileToAuthListModel = new List<FileInfoModel>();
            FollowedFileListModel = new List<FileInfoModel>();
        }

        #endregion

        #region Properties

        public int ParentId { get; set; }
        public int GParentFolderId { get; set; }
        public string ParentName { get; set; }
        public bool IsAuthorizer { get; set; }
        public bool IsAuthor { get; set; }
        public bool IsReader { get; set; }

        public FileInfoModel UploadFileModel { get; set; }
        public IList<FileInfoModel> FileListModel { get; set; }
        public IList<FileInfoModel> FileToAuthListModel { get; set; }
        public IList<FileInfoModel> FollowedFileListModel { get; set; }

        public InfoRepositoryFolderModel InfoFolder { get; set; }

        public IList<DirectoriesListModel> DirectoryListModel { get; set; }

        #endregion

        #region Nested classes

        public partial record FileInfoModel : BaseNopModel
        {
            public int Id { get; set; }
            public int OwnerId { get; set; }
            public string OwnerName { get; set; }
            public int ParentFolderId { get; set; }
            public string ParentFolderName { get; set; }
            public string FileName { get; set; }
            public string FileExtention { get; set; }
            public string Size { get; set; }
            public string Tags { get; set; }
            public bool Published { get; set; }
            public string PublishedTxt { get; set; }
            public int Status { get; set; }
            public string StatusTxt { get; set; }
            public int Version { get; set; }
            public bool FileFavEnabled { get; set; }
            public string FileCreated { get; set; }
            public string FileLastUpdate { get; set; }
        }

        public partial record DirectoriesListModel : BaseNopModel
        {
            public int Id { get; set; }
            public int ParentId { get; set; }
            public string DirectoryName { get; set; }
            public string AuthorName { get; set; }
            public string DirectoryLastUpdate { get; set; }
        }
        #endregion
    }
}