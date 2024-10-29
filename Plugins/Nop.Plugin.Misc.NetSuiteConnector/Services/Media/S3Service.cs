using Amazon;
using Amazon.Runtime.Internal.Util;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.StaticFiles;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Infrastructure;
using Nop.Services.Media;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Nop.Plugin.Misc.NetSuiteConnector.Services.Media
{
    public class S3Service : IS3Service
    {
        #region Fields

        private readonly NetSuiteConnectorSettings _netSuiteConnectorSettings;
        private readonly IProductService _productService;
        private readonly Nop.Services.Catalog.IProductService _nopProductService;
        private readonly Nop.Services.Logging.ILogger _logger;
        private readonly INopFileProvider _fileProvider;
        private readonly IPictureService _pictureService;
        private DateTime LastExecutionDateGeneral;
        #endregion

        #region Ctor

        public S3Service(NetSuiteConnectorSettings netSuiteConnectorSettings,
            IProductService productService,
            Nop.Services.Catalog.IProductService nopProductService,
            Nop.Services.Logging.ILogger logger, INopFileProvider fileProvider, IPictureService pictureService)
        {
            _netSuiteConnectorSettings = netSuiteConnectorSettings;
            _productService = productService;
            _nopProductService = nopProductService;
            _logger = logger;
            _fileProvider = fileProvider;
            _pictureService = pictureService;
        }

        #endregion

        #region Methods

        public void ImportImages()
        {
            LastExecutionDateGeneral = DateTime.Now;

            var accessKey = _netSuiteConnectorSettings.AccessKeyID;
            var secretKey = _netSuiteConnectorSettings.SecretAccessKey;

            var s3client = new AmazonS3Client(accessKey, secretKey, RegionEndpoint.USEast2);
            var buckets = Task.Run(async () => await s3client.ListBucketsAsync()).Result;

            string tempPath = _netSuiteConnectorSettings.ImagesTempFolder + "/tempS3Folder";
            GetObjectResponse objResponse = null;

            foreach (var bucket in buckets.Buckets)
            {
                var objects = Task.Run(async () => await s3client.ListObjectsAsync(bucket.BucketName)).Result;

                if (objects != null)
                {
                    foreach (var s3object in objects.S3Objects)
                    {
                            if (s3object.Key.Contains(_netSuiteConnectorSettings.DefaultS3Folder + "/"))
                        {
                            if (Directory.Exists(_netSuiteConnectorSettings.ImagesTempFolder))
                            {
                                if (!Directory.Exists(tempPath))
                                    Directory.CreateDirectory(tempPath);

                                objResponse = Task.Run(async () => await s3client.GetObjectAsync(new GetObjectRequest
                                {
                                    BucketName = bucket.BucketName,
                                    Key = s3object.Key
                                })).Result;

                                var task = Task.Run(async () => await objResponse.WriteResponseStreamToFileAsync(tempPath + "\\" + s3object.Key, false, CancellationToken.None));

                                while (!task.IsCompleted)
                                {

                                }
                            }
                            else
                            {
                                _logger.Warning("ImportProductError:: Folder " + _netSuiteConnectorSettings.ImagesTempFolder + " doesn't exists  LastExecutionDateGeneral:: " + LastExecutionDateGeneral);

                                throw new Exception("Folder " + _netSuiteConnectorSettings.ImagesTempFolder + " doesn't exists");
                            }
                        }
                    }
                }
            }

            objResponse.Dispose();

            var files = GetFiles(_netSuiteConnectorSettings.ImagesTempFolder, _netSuiteConnectorSettings.ImageTypes);

            var pictureList = new Dictionary<string, List<string>>();

            foreach (var file in files)
            {
                var fileName = file.Name.Split('_');
                var inventoryItemId = fileName.Length > 0 ? fileName[0] : string.Empty;

                if (!string.IsNullOrEmpty(inventoryItemId))
                {
                   
                    var product = _productService.GetProductByInventoryItem(inventoryItemId);

                    if (product != null)
                    {
                        var sku = product.Id.ToString();
                        var pictures = _nopProductService.GetProductPicturesByProductId(product.Id);

                        foreach (var picture in pictures)
                        {
                            _nopProductService.DeleteProductPicture(picture);
                        }

                        string picturePath = file.FullName;

                        if (!string.IsNullOrEmpty(picturePath))
                        {
                            if (System.IO.File.Exists(picturePath))
                            {
                                if (pictureList.ContainsKey(sku))
                                    pictureList[sku].Add(picturePath);
                                else
                                {
                                    pictureList.Add(sku, new List<string>());
                                    pictureList[sku].Add(picturePath);
                                }
                            }
                        }
                    }
                }
            }

            var pictureMetadataList = new List<ProductPictureMetadata>();

            foreach (var item in pictureList)
            {
                if(item.Key!=null)
                {
                    var pictureMetadata = new ProductPictureMetadata();
                    pictureMetadata.ProductItem = _nopProductService.GetProductById(Convert.ToInt32(item.Key));

                    for (int i = 0; i < item.Value.Count; i++)
                    {
                        if (i == 0)
                            pictureMetadata.Picture1Path = item.Value[i];
                        if (i == 1)
                            pictureMetadata.Picture2Path = item.Value[i];
                        if (i == 2)
                            pictureMetadata.Picture3Path = item.Value[i];
                    }

                    pictureMetadataList.Add(pictureMetadata);
                }
            }

            ImportProductImagesUsingServices(pictureMetadataList);
            Directory.Delete(tempPath, true);
        }

        #endregion

        #region Utilities

        protected List<FileInfo> GetFiles(string path, string searchPattern)
        {
            var baseFolder = new DirectoryInfo(path);
            string[] searchPatterns = searchPattern.Split('|');
            var files = new List<FileInfo>();
            foreach (string sp in searchPatterns)
                files.AddRange(baseFolder.GetFiles(sp, SearchOption.AllDirectories));

            return files.OrderBy(f => f.Name).ToList();
        }

        protected virtual void ImportProductImagesUsingServices(IList<ProductPictureMetadata> productPictureMetadata)
        {
            foreach (var product in productPictureMetadata)
            {
                
                foreach (var picturePath in new[] { product.Picture1Path, product.Picture2Path, product.Picture3Path })
                {
                    if (string.IsNullOrEmpty(picturePath))
                        continue;

                    var mimeType = GetMimeTypeFromFilePath(picturePath);
                    var newPictureBinary = _fileProvider.ReadAllBytes(picturePath);
                    var pictureAlreadyExists = false;
                    if (!product.IsNew)
                    {
                        //compare with existing product pictures
                        var existingPictures = _pictureService.GetPicturesByProductId(product.ProductItem.Id);
                        foreach (var existingPicture in existingPictures)
                        {
                            var existingBinary = _pictureService.LoadPictureBinary(existingPicture);
                            //picture binary after validation (like in database)
                            var validatedPictureBinary = _pictureService.ValidatePicture(newPictureBinary, mimeType);
                            if (!existingBinary.SequenceEqual(validatedPictureBinary) &&
                                !existingBinary.SequenceEqual(newPictureBinary))
                            {
                                _logger.Information("picture product ID" + product.ProductItem.Id);
                                continue;
                            }
                            //the same picture content
                            pictureAlreadyExists = true;
                            break;
                        }
                    }

                    if (pictureAlreadyExists)
                        continue;

                    try
                    {
                        var newPicture = _pictureService.InsertPicture(newPictureBinary, mimeType, _pictureService.GetPictureSeName(product.ProductItem.Name));
                        product.ProductItem.ProductPictures.Add(new ProductPicture
                        {
                            //EF has some weird issue if we set "Picture = newPicture" instead of "PictureId = newPicture.Id"
                            //pictures are duplicated
                            //maybe because entity size is too large
                            PictureId = newPicture.Id,
                            DisplayOrder = 1
                        });
                        _nopProductService.UpdateProduct(product.ProductItem);
                        //_logger.Information("picture Name_"+ product.ProductItem.Name);
                    }
                    catch (Exception ex)
                    {
                        LogPictureInsertError(picturePath, ex);
                    }
                }
            }
        }

        protected virtual string GetMimeTypeFromFilePath(string filePath)
        {
            new FileExtensionContentTypeProvider().TryGetContentType(filePath, out var mimeType);

            //set to jpeg in case mime type cannot be found
            return mimeType ?? MimeTypes.ImageJpeg;
        }

        protected void LogPictureInsertError(string picturePath, Exception ex)
        {
            var extension = _fileProvider.GetFileExtension(picturePath);
            var name = _fileProvider.GetFileNameWithoutExtension(picturePath);

            var point = string.IsNullOrEmpty(extension) ? string.Empty : ".";
            var fileName = _fileProvider.FileExists(picturePath) ? $"{name}{point}{extension}" : string.Empty;
            _logger.Error($"ImportProductError:: Insert picture failed (file name: {fileName}) LastExecutionDateGeneral:: " + LastExecutionDateGeneral, ex);

        }

        #endregion

        #region Nested Classes

        protected class ProductPictureMetadata
        {
            public Product ProductItem { get; set; }

            public string Picture1Path { get; set; }

            public string Picture2Path { get; set; }

            public string Picture3Path { get; set; }

            public bool IsNew { get; set; }
        }

        #endregion
    }
}
