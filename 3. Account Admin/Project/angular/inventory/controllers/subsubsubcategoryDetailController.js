(function () {
    'use strict';

    var controllerId = 'subsubsubcategoryDetailController';

    // Inject into main app module:
    angular.module('app')
        .controller(controllerId, [
            'categoryServices',
            'categoryModels',
            'sharedServices',
            'productServices',
            'accountServices',
            'imageServices',
            'searchServices',
            '$routeParams',
             subsubsubcategoryDetailController
        ]);

    function subsubsubcategoryDetailController(categoryServices, categoryModels, sharedServices, productServices, accountServices, imageServices, searchServices, $routeParams) {

        //Instantiate Controller as ViewModel
        var vm = this;

        //Default Properties: =============================
        vm.title = 'subsubsubcategoryDetailController';
        vm.activate = activate;
        vm.headerBGColor = "#F5F5F5";
        vm.productView = "grid";

        //Route Properties =============================
        vm.categoryAttribute = $routeParams.categoryAttribute; //$route.current.params.attrbute; 
        vm.subcategoryAttribute = $routeParams.subcategoryAttribute; //$route.current.params.attrbute; 
        vm.subsubcategoryAttribute = $routeParams.subsubcategoryAttribute; //$route.current.params.attrbute; 
        vm.subsubsubcategoryAttribute = $routeParams.subsubsubcategoryAttribute; //$route.current.params.attrbute; 


        /* ==========================================
               Product Pagination & Load More
        ==========================================*/

        vm.amountToSkip = 0;
        vm.amountPerPage = 48;
        vm.resultsAvailable = 0;
        vm.searchingMore = false;

        $(window).scroll(function () {
            if ($(window).scrollTop() + $(window).height() == $(document).height()) {

                if (vm.amountToSkip < vm.resultsAvailable && vm.products != null && vm.searchingMore == false) {

                    vm.searchingMore = true;

                    var locationPath = vm.subsubsubcategory.FullyQualifiedName;

                    searchServices.searchProducts('', "locationPath eq '" + locationPath + "'", vm.productSorting.OrderByString, vm.amountToSkip, vm.amountPerPage)
                    .success(function (data, status, headers, config) {

                        //vm.searchQueryCompleted = vm.searchQuery;
                        vm.searchingMore = false;
                        vm.resultsAvailable = data.Count;

                        //vm.paginationInfo.lastResultCount = data.Results.length;


                        vm.amountToSkip = vm.amountToSkip + data.Results.length;

                        //Append the data
                        vm.products.Results = vm.products.Results.concat(data.Results); //= .push(data.Results);// = [].concat(vm.searchResults, data);
                        

                    })
                    .error(function (data, status, headers, config) {

                    })
                }
            }
        });


        /* ==========================================
               Product View
        ==========================================*/
        vm.showGridView = function () {
            vm.productView = "grid";
        }
        vm.showListView = function () {
            vm.productView = "list";
        }

        /* ==========================================
           Product Sorting
       ==========================================*/

        vm.reloadingProducts = false;
        vm.productSortables = [];
        vm.productSorting = { SortLabel: "Date added (Newest)", OrderByString: "dateCreated desc" }

        vm.getProductSortables = function () {
            searchServices.getSortables()
            .success(function (data, status, headers, config) {

                vm.productSortables = data;

                //Remove the first item from the array ("Relevance")
                vm.productSortables.shift();

                //Inject Custom Ordering (if allowed)
                if (vm.account.PaymentPlan.AllowCustomOrdering) {
                    vm.productSortables.push({ SortLabel: "Custom order", OrderByString: "orderId asc" });
                }

                //Set the first type as default ("Date Created (Newest)")
                vm.productSorting = data[0];

            })
            .error(function (data, status, headers, config) {
            })
        }

        vm.updateProductSorting = function (sortable) {
            vm.productSorting = sortable;
            vm.reloadingProducts = true;
            vm.getProducts(false);
        }

        /* ==========================================
           Featured Properties
        ==========================================*/

        vm.featuredProperties = null;

        vm.getFeatured = function () {
            searchServices.getFeatured()
            .success(function (data, status, headers, config) {

                vm.featuredProperties = data;

            })
            .error(function (data, status, headers, config) {
            })
        }

        /* ==========================================
               Helper Methods
        ==========================================*/

        // Debug Methods ===================================
        // wrap console.log() within the "Debug" namespace
        //    -->  Logs can be removed during minification
        var Debug = {};
        Debug.trace = function (message) {
            console.log(message);
        };



        //Account User:
        vm.currentUserProfile = null;
        var currentUserRoleIndex = null //<-- used internally to check role access, must be updated when getting or refreshing the user.
        var userRoles = []; //<-- used internally to check role access, must be updated when getting or refreshing the user.
        //--------------------------------------------------------------------

        /* ==========================================
             Core Service Properties
        ==========================================*/

        // vm.TrialHoldDays = null; //<----comes from CoreServices (via local feed)
        //vm.CustodianFrequencyDescription = null; //<----comes from CoreServices (via local feed)
        //vm.UnverifiedAccountsDaysToHold = null; //<----comes from CoreServices (via local feed)
        //vm.PlatformWorkerFrequencyDescription = null; //<----comes from CoreServices (via local feed)



        /* ==========================================
                Controller Properties
        ==========================================*/




        /* ==========================================
               Controller Models
       ==========================================*/






        /* ==========================================
           SUBSUBSUBCATEGORY NAME
        ==========================================*/

        vm.SubsubsubcategoryNameStatus =
            {
                Updating: false,
                Editing: false,
                SendingComplete: false,

                SubsubsubcategoryName: null,

                Results: {
                    IsSuccess: false,
                    Message: null
                },
                Retry: function () {
                    this.Updating = false;
                    this.Editing = true;
                    this.SendingComplete = false;
                },

                Clear: function () {

                    //Debug.trace("Clearing update subsubsubcategory name form data.");
                    this.Updating = false;
                    this.Editing = false;
                    this.SendingComplete = false;
                }


            }

        vm.editSubsubsubcategoryName = function () {
            //Debug.trace("Editing subsubsubcategory name...");

            vm.SubsubsubcategoryNameStatus.NewSubsubsubcategoryName = vm.subsubsubcategory.SubsubsubcategoryName;

            vm.SubsubsubcategoryNameStatus.Editing = true;

        }

        vm.cancelUpdateSubsubsubcategoryName = function () {

            vm.SubsubsubcategoryNameStatus.Clear();

        }

        vm.updateSubsubsubcategoryName = function () {
            //Debug.trace("Updating subsubsubcategory name...");

            vm.SubsubsubcategoryNameStatus.Updating = true;
            vm.SubsubsubcategoryNameStatus.Editing = false;

            categoryServices.updateSubsubsubcategoryName(vm.subsubsubcategory.SubsubsubcategoryID, vm.SubsubsubcategoryNameStatus.NewSubsubsubcategoryName)
           .success(function (data, status, headers, config) {

               vm.SubsubsubcategoryNameStatus.Updating = false;
               vm.SubsubsubcategoryNameStatus.SendingComplete = true;

               if (data.isSuccess) {

                   vm.SubsubsubcategoryNameStatus.Results.IsSuccess = true;
                   vm.SubsubsubcategoryNameStatus.Results.Message = "Subsubsubcategory name has been changed.";

                   //Update SubsubsubcategoryName and SubsubsubcategoryNameKey 
                   vm.subsubsubcategory.SubsubsubcategoryName = vm.SubsubsubcategoryNameStatus.NewSubsubsubcategoryName;
                   vm.subsubsubcategory.SubsubsubcategoryNameKey = data.Results[0];

                   vm.SubsubsubcategoryNameStatus.Clear();


               }
               else {
                   vm.SubsubsubcategoryNameStatus.Results.IsSuccess = false;
                   vm.SubsubsubcategoryNameStatus.Results.Message = data.ErrorMessage;

               }

           }).error(function (data, status, headers, config) {

               vm.SubsubsubcategoryNameStatus.IsSuccess = false;
               vm.SubsubsubcategoryNameStatus.Results.Message = "An error has occurred while using the service.";
           })

        }

        vm.cancelSubsubsubcategoryNameEdit = function () {

            vm.SubsubsubcategoryNameStatus.Editing = false;
            vm.SubsubsubcategoryNameStatus.Updating = false;
        }

        /* ==========================================
           END SUBSUBSUBCATEGORY NAME
        ==========================================*/





        /* ==========================================
           MANAGE DESCRIPTION
        ==========================================*/

        vm.descriptionNewCopy = null;
        vm.descriptionIsEditing = false;
        vm.descriptionIsUpdating = false;
        vm.descriptionSendingComplete = false;
        vm.descriptionIsSuccess = false;
        vm.descriptionResultsMessage = null;

        vm.editDescription = function () {

            vm.descriptionNewCopy = vm.subsubsubcategory.Description;
            vm.descriptionIsEditing = true;

        }

        vm.cancelDescription = function () {

            vm.descriptionNewCopy = null;
            vm.descriptionIsEditing = false;
            vm.descriptionIsUpdating = false;
            vm.descriptionSendingComplete = false;
            vm.descriptionIsSuccess = false;
            vm.descriptionResultsMessage = null;
        }

        vm.updateDescription = function () {

            vm.descriptionIsUpdating = true;
            vm.descriptionIsEditing = false;

            categoryServices.updateSubsubsubcategoryDescription(vm.subsubsubcategory.SubsubsubcategoryID, vm.descriptionNewCopy)
           .success(function (data, status, headers, config) {

               vm.descriptionIsUpdating = false;
               vm.descriptionSendingCompletee = true;

               if (data.isSuccess) {

                   vm.descriptionIsSuccess = true;

                   vm.subsubsubcategory.Description = vm.descriptionNewCopy;

                   vm.cancelDescription();

               }
               else {
                   vm.descriptionIsSuccess = false;
                   vm.descriptionResultsMessage = data.ErrorMessage;

               }

           }).error(function (data, status, headers, config) {

               vm.descriptionIsSuccess = false;
               vm.descriptionResultsMessage = "An error has occurred while using the service.";
           })

        }








        /* =========================================

            CHANGE VISIBILITY

            ========================================*/

        vm.makeVisible = function () {
            vm.subsubsubcategory.Visible = null;

            categoryServices.updateSubsubsubcategoryVisibleState(vm.subsubsubcategory.SubsubsubcategoryID, true)
            .success(function (data, status, headers, config) {

                if (data.isSuccess) {
                    vm.subsubsubcategory.Visible = true;
                }
                else {
                    vm.subsubsubcategory.Visible = false;
                }
            })
                .error(function (data, status, headers, config) {
                    vm.subsubsubcategory.Visible = false;
                })
        }

        vm.makeHidden = function () {
            vm.subsubsubcategory.Visible = null;

            categoryServices.updateSubsubsubcategoryVisibleState(vm.subsubsubcategory.SubsubsubcategoryID, false)
            .success(function (data, status, headers, config) {

                if (data.isSuccess) {
                    vm.subsubsubcategory.Visible = false;
                }
                else {
                    vm.subsubsubcategory.Visible = true;
                }
            })
                .error(function (data, status, headers, config) {
                    vm.subsubsubcategory.Visible = true;
                })
        }




        /* ==========================================

           DELETE SUBSUBSUBCATEGORY

       ==========================================*/

        vm.subsubsubcategoryDeletion =
            {
                Verify: false,
                Complete: false,
                Processing: false,
                IsSuccess: false,
                Message: null,

                reset: function () {
                    this.Complete = false,
                    this.IsSuccess = false,
                    this.Message = null,
                    this.Processing = false
                    this.Verify = false;
                }
            }

        vm.startDeletion = function () {
            vm.subsubsubcategoryDeletion.Verify = true;
        }

        vm.cancelDeletion = function () {
            vm.subsubsubcategoryDeletion.reset();
        }

        vm.deleteSubsubsubcategory = function () {

            vm.subsubsubcategoryDeletion.Verify = false;
            vm.subsubsubcategoryDeletion.Processing = true;

            //Debug.trace("Deleting subsubsubcategory...");

            categoryServices.deleteSubsubsubcategory(vm.subsubsubcategory.SubsubsubcategoryID)
            .success(function (data, status, headers, config) {

                vm.subsubsubcategoryDeletion.Processing = false;
                vm.subsubsubcategoryDeletion.Complete = true;

                if (data.isSuccess) {

                    vm.subsubsubcategoryDeletion.IsSuccess = true;
                    vm.subsubsubcategoryDeletion.Message = "This subsubsubcategory has been deleted.";
                }
                else {

                    vm.subsubsubcategoryDeletion.IsSuccess = false;
                    vm.subsubsubcategoryDeletion.Message = data.ErrorMessage;
                }

            })
                .error(function (data, status, headers, config) {

                    vm.subsubsubcategoryDeletion.Processing = false;
                    vm.subsubsubcategoryDetailUpdates.IsSuccess = false;
                    vm.subsubsubcategoryDetailUpdates.Complete = true;
                    vm.subsubsubcategoryDetailUpdates.Message = "An error occurred while attempting to use the service...";

                })


        }

        /* ==========================================

          END DELETE SUBSUBSUBCATEGORY

       ==========================================*/



































        /*=====================================================================
        ========================================================================
        ========================================================================

        START IMAGING METHODS
       
        
        ========================================================================*/


        /* ==========================================

          LOAD IMAGES

       ==========================================*/

        vm.imageObjectType = "subsubsubcategory"; //<-- Must update on different TYPES
        vm.objectId = null; //<-- Load once and reuse
        vm.imagesReloaded = false;

        vm.imageRecords = null;
        vm.mainThumbnailUrl = null;

        vm.getImageRecords = function () {

            vm.objectId = vm.subsubsubcategory.SubsubsubcategoryID;

            imageServices.getImageRecordsForObject(vm.imageObjectType, vm.objectId, false)
            .success(function (data, status, headers, config) {

                vm.imageRecords = data;

                //If gallery ordering was just updated we reset details in modal window
                //if (vm.galleryUpdated)
                //{
                //vm.galleryUpdated = false;
                //vm.galleryUpdated = false;
                //vm.loadImageDetails(vm.imageGroupNameKeyDetails, vm.imageGroupNameDetails, )
                //}

                //Turn on carousels for image gallery record types
                setTimeout(function () {

                    vm.imageRecords.forEach(function (group) {
                        group.ImageRecords.forEach(function (record) {
                            if (record.Type == 'gallery') {
                                var imageGalleryDivId = "carousel-" + group.GroupNameKey + "-" + record.FormatNameKey;

                                $("#" + imageGalleryDivId).carousel();

                            }

                            //Pull out main thumbnail
                            if (group.GroupNameKey == "default" && record.FormatNameKey == "thumbnail") {
                                vm.mainThumbnailUrl = record.Url;
                                angular.element(document.querySelector("#mainThumbnail")).scope().$apply();
                                //scope.$apply(); //<-- force UI refresh
                            }
                        });
                    });

                }, 100);

            })
            .error(function (data, status, headers, config) {

            })

        }

        /* ==========================================

          IMAGE DETAILS

       ==========================================*/

        vm.imageGroupNameKeyDetails = null;
        vm.imageGroupNameDetails = null;
        vm.imageRecordDetails = null;
        vm.resetImageDetailsTab = true;

        vm.loadImageDetails = function (imageGroupNameKey, imageGroupName, imageRecord) {
            vm.imageRecordDeleteApproval = false;
            vm.imageRecordDeleting = false;
            vm.imageGalleryIndexToDelete = null;

            vm.resetImageDetailsTab = true;

            //Debug.trace(imageRecord);

            vm.imageGroupNameKeyDetails = imageGroupNameKey;
            vm.imageGroupNameDetails = imageGroupName;
            vm.imageRecordDetails = imageRecord;

            // -- Reset image editing vars:
            vm.resetAllImageEditingVars();
        }


        /* ==========================================

           UPLOAD SOURCE IMAGE / Interact with Image Editng Modal to Upload Image Records

        ==========================================*/

        //vm.intermediateUrl = "init"; //<--After successful upload this is updated with the intermediary URL of the image to edit before creating a record

        //vm.uploadingIntermediaryImage = false;

        vm.firstImageUpload = true;

        vm.intermediaryWidth = null;
        vm.intermediaryHeight = null;

        //*Helper Methods -----------------------------------------------------------------------------------

        //Add function to image record/format slugs to trigger hidden file input when clicked
        vm.initiateIntermediaryImageUpload = function (imageFormat, imageGroupKey) {

            //vm.uploadingIntermediaryImage = true;

            vm.intermediaryTypeKey = vm.imageObjectType;
            vm.intermediaryGroupKey = imageGroupKey;

            //Debug.trace(imageGroupKey);
            vm.intermediaryWidth = imageFormat.Width;
            vm.intermediaryHeight = imageFormat.Height;
            vm.intermediaryFormatKey = imageFormat.FormatNameKey;
            vm.intermediaryFormatName = imageFormat.FormatName;


            //Debug.trace("initiating upload for " + imageFormat.FormatName);

            //Click's upload button on hidden file input
            $("#imageUploader:hidden").trigger('click');

        }

        // Add events to input to fire when status changes (MUST BE IN PARENT 'PARTIAL' - NOT CHILD 'TEMPLATE')
        document.getElementById("imageUploader").addEventListener('change', uploadIntermediaryImage);

        // Grab the files and set them to our variable and open the editing modal window
        function uploadIntermediaryImage() {

            //SHow intermediary loader:
            $('#intermediaryImageUploading-' + vm.intermediaryGroupKey).fadeIn(200);
            //$('#imageThumbnailsPanel-' + vm.intermediaryGroupKey).hide();

            /////////////////////////////////////////////

            //Fade out/clear all hover & error messages
            //$("#profilePhotoOverlay").fadeOut(150);
            //$("#photoUploadErrorText").text("");
            //$("#photoUploadErrorMessage").slideUp();

            /////////////////////////////////////////////

            // Check for the various HTML5 File API support.
            //if (window.File && window.FileReader && window.FileList && window.Blob) {

            if (window.File && window.FileReader && window.FileList && window.Blob) {

                // Great! All the File APIs are supported.


                //Debug.trace("Uploading Image...");// + vm.uploadingPhoto);

                var file = document.getElementById('imageUploader').files[0];

                //Debug.trace("photo ready for upload");
                //Debug.trace(file);
                //Debug.trace("source=" + file.src);

                //Debug.trace("size=" + file.size);
                //Debug.trace("type=" + file.type);
                //Debug.trace("result=" + file.result);

                if (file.size > 5000000) {
                    //Hide intermediary loader:
                    $('#intermediaryImageUploading-' + vm.intermediaryGroupKey).hide();

                    //showNotification
                    $('#intermediaryUploadErrorImage-' + vm.intermediaryGroupKey).slideDown();
                    $('#intermediaryUploadErrorImage-' + vm.intermediaryGroupKey).text("Images must be smaller than 5mb");
                    setTimeout(function () {
                        $('#intermediaryUploadErrorImage-' + vm.intermediaryGroupKey).slideUp();
                    }, 2000);

                    document.getElementById('imageUploader').value = '';

                }
                else if (file.type != "image/jpeg" && file.type != "image/png" && file.type != "image/gif" && file.type != "image/bmp") { // <-- Tiff features in future && file.type != "image/tiff"

                    //Debug.trace("Not a supported image format");
                    //vm.uploadingIntermediaryImage = false;

                    //Hide intermediary loader:
                    $('#intermediaryImageUploading-' + vm.intermediaryGroupKey).hide();
                    //$('#imageThumbnailsPanel-' + vm.intermediaryGroupKey).show();

                    //showNotification("Not a supported file type!", "Alert", null, true);

                    $('#intermediaryUploadErrorImage-' + vm.intermediaryGroupKey).slideDown();
                    $('#intermediaryUploadErrorImage-' + vm.intermediaryGroupKey).text(file.type + " is not a supported file type!");
                    setTimeout(function () {
                        $('#intermediaryUploadErrorImage-' + vm.intermediaryGroupKey).slideUp();
                    }, 2000);


                    document.getElementById('imageUploader').value = '';

                    /////////////////////////////////////////////

                    //vm.uploadingPhoto = false;
                    //$("#uploadingPhotoAnimation").fadeOut(100);
                    //$("#photoUploadErrorText").text("Please only upload an image of type Jpeg, Png, Gif or Bmp.");
                    //$("#photoUploadErrorMessage").slideDown('slow').delay(1900).slideUp('slow');

                    /////////////////////////////////////////////

                }
                    //else if (file.size > "5000000") // (in Bytes) (5mb) 
                    //{
                    // //Debug.trace("File too large");
                    //}
                else {

                    var type = "jpg"; //<-- JPG & BMP will always convert to jpg

                    if (file.type == "image/png") {
                        type = "png";
                        document.getElementById("imageEditingFrame").style.height = "570px";
                    }
                    else if (file.type == "image/gif") {
                        type = "gif";
                        document.getElementById("imageEditingFrame").style.height = "570px";
                    }
                    else {
                        document.getElementById("imageEditingFrame").style.height = "490px";
                    }



                    //Show Loader

                    /////////////////////////////////////////////

                    //$("#uploadingPhotoAnimation").fadeIn(200);

                    /////////////////////////////////////////////               

                    imageServices.uploadIntermediaryImageForObjectRecord(file, type, vm.intermediaryWidth, vm.intermediaryHeight).onreadystatechange = function () {
                        if (this.readyState == 4) { //<--0 = notinitialized, 1 = set up, 2 = sent, 3 = in process, 4 = complete

                            //Hide intermediary loader:
                            $('#intermediaryImageUploading-' + vm.intermediaryGroupKey).slideUp(1000);
                            //$('#imageThumbnailsPanel-' + vm.intermediaryGroupKey).show();


                            if (this.response != null && this.response != '') {

                                //Debug.trace(this);
                                //Debug.trace("response1:" + this.response);
                                var data = JSON.parse(this.response);

                                if (data.isSuccess) {

                                    var editingUrl =
                                        '/Imaging/Instructions/' + '?SourceContainerName=' + data.SourceContainerName +
                                        '&ImageID=' + data.ImageId +
                                        '&FileName=' + data.FileName + '&FormatWidth=' + vm.intermediaryWidth + '&FormatHeight=' + vm.intermediaryHeight +
                                        '&SourceWidth=' + data.SourceWidth + '&SourceHeight=' + data.SourceHeight +
                                        '&ObjectType=' + vm.imageObjectType +
                                        '&ObjectID=' + vm.objectId +
                                        '&ImageGroupNameKey=' + vm.intermediaryGroupKey +
                                        '&ImageFormatNameKey=' + vm.intermediaryFormatKey +
                                        '&Type=' + type;

                                    //window.location.href = '/Imaging/Instructions/' + '?SourceContainerName=' + data.SourceContainerName + '&FileName=' + data.FileName + '&FormatWidth=' + vm.intermediaryWidth + '&FormatHeight=' + vm.intermediaryHeight;

                                    //Debug.trace(editingUrl);

                                    document.getElementById('imageEditingFrame').src = editingUrl;

                                    $('#imageEditModal').modal('show');
                                    $('#imageEditModal').modal({ show: true });

                                    //Add event to refresh images view on modal close (only once)
                                    if (vm.firstImageUpload) {
                                        vm.imagesReloaded = true; //<-- start from last image in gallery carousel view going forward

                                        $('#imageEditModal').on('hidden.bs.modal', function () {
                                            //alert("test");
                                            vm.getImageRecords();
                                            //vm.uploadingIntermediaryImage = false;
                                        })

                                        //vm.firstImageUpload = false;
                                    }


                                    /**/
                                    //Clear input box in case same image is uploaded into a new slug:
                                    document.getElementById('imageUploader').value = '';

                                }
                                else {


                                }
                            }
                        }
                        else {

                            if (this.response != null && this.response != '') {
                                //Debug.trace("response2:" + this.response);

                                var data = JSON.parse(this.response);

                                if (data.isSuccess == false) {
                                    //Clear input box in case same image is uploaded into a new slug:
                                    document.getElementById('imageUploader').value = '';

                                    //console.log("status:" + this.status);
                                    //console.log("readystate:" + this.readyState);
                                    //Hide intermediary loader:
                                    //$('#intermediaryImageUploading-' + vm.intermediaryGroupKey).fadeOut();
                                    //$('#imageThumbnailsPanel-' + vm.intermediaryGroupKey).show();

                                    //Hide intermediary loader:
                                    $('#intermediaryImageUploading-' + vm.intermediaryGroupKey).hide();
                                    //$('#imageThumbnailsPanel-' + vm.intermediaryGroupKey).show();

                                    //showNotification("Not a supported file type!", "Alert", null, true);

                                    $('#intermediaryUploadErrorImage-' + vm.intermediaryGroupKey).slideDown();
                                    $('#intermediaryUploadErrorImage-' + vm.intermediaryGroupKey).text(data.ErrorMessage);
                                    setTimeout(function () {
                                        $('#intermediaryUploadErrorImage-' + vm.intermediaryGroupKey).slideUp();
                                    }, 5500);
                                }
                            }

                        }
                    }

                }

        }
        else {
            console.log("File API not supported in this browser");
        }
    }


        /* ==========================================

          Edit Titles/Descriptions

        ==========================================*/

        // - Single Image Vars

        vm.imageTitleEditing = false;
        vm.newImageRecordTitle = null;
        vm.imageTitleProcessing = false;

        vm.imageDescriptionEditing = false;
        vm.newImageRecordDescription = null;
        vm.imageDescriptionProcessing = false;

        // - Gallery Image Vars

        vm.imageGalleryTitleEditing = false;
        vm.newImageRecordGalleryTitle = null;
        vm.imageGalleryTitleProcessing = false;

        vm.imageGalleryDescriptionEditing = false;
        vm.newImageRecordGalleryDescription = null;
        vm.imageGalleryDescriptionProcessing = false;

        vm.visibleGalleryTitleEditingIndex = null;
        vm.visibleGalleryDescriptionEditingIndex = null;

        // - Global State   

        vm.globalImageEditingState = false;

        // -- Reset All Vars -------

        vm.resetAllImageEditingVars = function () {
            vm.imageTitleEditing = false;
            vm.newImageRecordTitle = null;
            vm.imageTitleProcessing = false;

            vm.imageDescriptionEditing = false;
            vm.newImageRecordDescription = null;
            vm.imageDescriptionProcessing = false;

            vm.imageGalleryTitleEditing = false;
            vm.newImageRecordGalleryTitle = null;
            vm.imageGalleryTitleProcessing = false;

            vm.imageGalleryDescriptionEditing = false;
            vm.newImageRecordGalleryDescription = null;
            vm.imageGalleryDescriptionProcessing = false;

            vm.visibleGalleryTitleEditingIndex = null;
            vm.visibleGalleryDescriptionEditingIndex = null;

            vm.globalImageEditingState = false;

            vm.visibleGalleryUrlIndex = null;
        }

        //-- Single Image Title --------------------------

        vm.editImageTitle = function () {
            vm.newImageRecordTitle = vm.imageRecordDetails.Title;
            vm.imageTitleEditing = true;
            vm.globalImageEditingState = true;
        }

        vm.updateImageTitle = function () {

            //Debug.trace("Updating image title...");

            vm.imageTitleEditing = false;
            vm.imageTitleProcessing = true;

            imageServices.updateImageTitle(vm.imageObjectType, vm.objectId, vm.imageGroupNameKeyDetails, vm.imageRecordDetails.FormatNameKey, vm.newImageRecordTitle)
           .success(function (data, status, headers, config) {

               if (data.isSuccess) {

                   vm.imageRecordDetails.Title = vm.newImageRecordTitle;
                   vm.resetAllImageEditingVars();
                   vm.getImageRecords();
               }
               else {
                   vm.resetAllImageEditingVars();
               }

           }).error(function (data, status, headers, config) {
               vm.resetAllImageEditingVars();
           })
        }


        //-- Single Image Description --------------------------

        vm.editImageDescription = function () {
            vm.newImageRecordDescription = vm.imageRecordDetails.Description;
            vm.imageDescriptionEditing = true;
            vm.globalImageEditingState = true;
        }

        vm.updateImageDescription = function () {

            //Debug.trace("Updating image description...");

            vm.imageDescriptionEditing = false;
            vm.imageDescriptionProcessing = true;

            imageServices.updateImageDescription(vm.imageObjectType, vm.objectId, vm.imageGroupNameKeyDetails, vm.imageRecordDetails.FormatNameKey, vm.newImageRecordDescription)
           .success(function (data, status, headers, config) {

               if (data.isSuccess) {

                   vm.imageRecordDetails.Description = vm.newImageRecordDescription;
                   vm.resetAllImageEditingVars();
                   vm.getImageRecords();
               }
               else {
                   vm.resetAllImageEditingVars();
               }

           }).error(function (data, status, headers, config) {
               vm.resetAllImageEditingVars();
           })
        }

        //-- Gallery Image Title --------------------------

        vm.editImageGalleryTitle = function (index) {

            vm.visibleGalleryTitleEditingIndex = index;

            vm.newImageRecordGalleryTitle = vm.imageRecordDetails.GalleryImages[index].Title;
            vm.imageGalleryTitleEditing = true;
            vm.globalImageEditingState = true;
        }

        vm.updateImageGalleryTitle = function (index) {

            //Debug.trace("Updating image gallery title...");

            vm.imageGalleryTitleEditing = false;
            vm.imageGalleryTitleProcessing = true;

            imageServices.updateGalleryTitle(vm.imageObjectType, vm.objectId, vm.imageGroupNameKeyDetails, vm.imageRecordDetails.FormatNameKey, index, vm.newImageRecordGalleryTitle)
           .success(function (data, status, headers, config) {

               if (data.isSuccess) {

                   vm.imageRecordDetails.GalleryImages[index].Title = vm.newImageRecordGalleryTitle;
                   vm.resetAllImageEditingVars();
                   vm.getImageRecords();
               }
               else {
                   vm.resetAllImageEditingVars();
               }

           }).error(function (data, status, headers, config) {
               vm.resetAllImageEditingVars();
           })
        }


        //-- Gallery Image Description --------------------------

        vm.editImageGalleryDescription = function (index) {

            vm.visibleGalleryDescriptionEditingIndex = index;

            vm.newImageRecordGalleryDescription = vm.imageRecordDetails.GalleryImages[index].Description;
            vm.imageGalleryDescriptionEditing = true;
            vm.globalImageEditingState = true;
        }

        vm.updateImageGalleryDescription = function (index) {

            //Debug.trace("Updating image gallery description...");

            vm.imageGalleryDescriptionEditing = false;
            vm.imageGalleryDescriptionProcessing = true;

            imageServices.updateGalleryDescription(vm.imageObjectType, vm.objectId, vm.imageGroupNameKeyDetails, vm.imageRecordDetails.FormatNameKey, index, vm.newImageRecordGalleryDescription)
           .success(function (data, status, headers, config) {

               if (data.isSuccess) {

                   vm.imageRecordDetails.GalleryImages[index].Description = vm.newImageRecordGalleryDescription;
                   vm.resetAllImageEditingVars();
                   vm.getImageRecords();
               }
               else {
                   vm.resetAllImageEditingVars();
               }

           }).error(function (data, status, headers, config) {
               vm.resetAllImageEditingVars();
           })
        }



        /* ==========================================
               
            Reorder Gallery
               
        ==========================================*/

        vm.newGalleryImageOrder = null;
        vm.orgGalleryImageOrder = null;
        vm.imageOrderProcessing = false;

        //vm.galleryUpdated = false;

        vm.initializeImageGalleryOrderPanel = function () {

            //vm.newGalleryImageOrder = vm.imageRecordDetails.GalleryImages;

            vm.cancelImageOrdering();
            vm.newGalleryImageOrder = [];
            vm.orgGalleryImageOrder = [];

            //vm.newOrder = vm.category.slice(); //<-- Depricated

            //Pipe in values and rename to generic items----------------
            for (var i = 0, len = vm.imageRecordDetails.GalleryImages.length; i < len; ++i) {
                var galleryImage = { index: i, url: vm.imageRecordDetails.GalleryImages[i].Url_sm };
                vm.newGalleryImageOrder.push(galleryImage);
            };

            vm.orgGalleryImageOrder = vm.newGalleryImageOrder;
        }

        vm.cancelImageOrdering = function () {
            vm.newGalleryImageOrder = null;
            vm.orgGalleryImageOrder = null;
        }

        vm.updateImageGalleryOrdering = function () {

            vm.imageOrderProcessing = true;

            //Debug.trace("Updating image gallery ordering...");

            //Create array of indexes in the new order ----
            var indexList = vm.newGalleryImageOrder.map(function (item) {
                return item['index'];
            });

            vm.imageGalleryOrderProcessing = true;

            imageServices.reorderGallery(vm.imageObjectType, vm.objectId, vm.imageGroupNameKeyDetails, vm.imageRecordDetails.FormatNameKey, indexList)
           .success(function (data, status, headers, config) {

               vm.imageOrderProcessing = false;

               if (data.isSuccess) {

                   $('#imageOrderingSuccess').fadeIn();

                   setTimeout(function () {
                       $('#imageOrderingSuccess').fadeOut();
                   }, 1800);

                   //vm.imageRecordDetails.GalleryImages[index].Description = vm.newImageRecordGalleryDescription;
                   //vm.resetAllImageEditingVars();
                   //vm.galleryUpdated = true; //<-- will update ordering of gallery detail modal if true after updating records from CoreServices (below)
                   vm.getImageRecords();

                   //Reorder items in details

                   //Pipe in values to reorder items in details----------------
                   var newGalleryImagesArray = [];

                   for (var i = 0, len = vm.newGalleryImageOrder.length; i < len; ++i) {
                       newGalleryImagesArray.push(vm.imageRecordDetails.GalleryImages[vm.newGalleryImageOrder[i].index]);
                   };

                   vm.imageRecordDetails.GalleryImages = newGalleryImagesArray;

               }
               else {
                   //vm.resetAllImageEditingVars();
               }

           }).error(function (data, status, headers, config) {

               vm.imageOrderProcessing = false;
               //vm.resetAllImageEditingVars();
           })
        }

        /* ==========================================
       
          Show/Hide Gallery Image URLs
       
        ==========================================*/

        vm.visibleGalleryUrlIndex = null;

        vm.setVisibleGalleryUrlIndex = function (indexValue) {
            vm.visibleGalleryUrlIndex = indexValue;
        }

        /* ==========================================

          DELETE/CLEAR IMAGE RECORD

        ==========================================*/

        vm.imageRecordDeleteApproval = false;
        vm.imageRecordDeleting = false;

        vm.approveImageRecordDeletion = function () {
            vm.imageRecordDeleteApproval = true;
            vm.imageRecordDeleting = false;
        }

        vm.disproveImageRecordDeletion = function () {
            vm.imageRecordDeleteApproval = false;
            vm.imageRecordDeleting = false;
        }

        vm.deleteImageRecord = function () {

            vm.imageRecordDeleting = true;

            imageServices.deleteImageRecordForObject(vm.imageObjectType, vm.objectId, vm.imageGroupNameKeyDetails, vm.imageRecordDetails.FormatNameKey)
            .success(function (data, status, headers, config) {

                vm.getImageRecords();
                $('.modal.in').modal('hide');

                //Refresh main thumnail:
                angular.element(document.querySelector("#mainThumbnail")).scope().$apply();
            })
            .error(function (data, status, headers, config) {
                vm.imageRecordDeleteApproval = false;
                vm.imageRecordDeleting = false;
            })

        }




        /* ==========================================

          DELETE/CLEAR IMAGE GALLERY ITEM

        ==========================================*/

        vm.imageGalleryIndexToDelete = null;

        vm.deleteImageGalleryItem = function (imageIndex) {

            vm.imageGalleryIndexToDelete = imageIndex;

            imageServices.deleteGalleryImageForObject(vm.imageObjectType, vm.objectId, vm.imageGroupNameKeyDetails, vm.imageRecordDetails.FormatNameKey, imageIndex)
            .success(function (data, status, headers, config) {

                if (data.isSuccess) {
                    vm.imageRecordDetails.GalleryImages.splice(imageIndex, 1);
                }

                vm.imageGalleryIndexToDelete = null;
                vm.getImageRecords();

                //If this was the last image in the array we close the modal:
                if (vm.imageRecordDetails.GalleryImages.length == 0) {
                    $('.modal.in').modal('hide');
                }


            })
            .error(function (data, status, headers, config) {
                vm.imageGalleryIndexToDelete = null;
            })

        }

        /*=====================================================================


        END IMAGING METHODS
       
        ========================================================================
        ========================================================================
        ========================================================================*/








































        /*========================================
          
          PRODUCT METHODS
         
         ========================================*/


        vm.productConstraint = false; //<-- True if plan needs an upgrade


        vm.newProduct =
             {
                 Visible: true,
                 Name: null,

                 // Service Processing --------

                 IsSending: false,
                 SendingComplete: false,

                 Results: {
                     IsSuccess: false,
                     Message: null
                 },

                 // Visibiliy ----

                 Hide: function () {
                     this.Visible = false;
                 },

                 Show: function () {
                     this.Visible = true;
                 },

                 // Cleanup Routine(s) ----------

                 Retry: function () {
                     this.IsSending = false;
                     this.SendingComplete = false;
                 },

                 Clear: function () {
                     //Debug.trace("Clearing new product form data.");

                     this.Name = null;
                     this.Visible = true;

                     this.IsSending = false;
                     this.SendingComplete = false;

                     this.Results.IsSuccess = false;
                     this.Results.Message = null;
                 }
             }

        vm.newProductPath = null;
        vm.createProduct = function () {

            vm.productConstraint = false;
            vm.newProduct.IsSending = true;

            //Debug.trace("Creating product...");

            var locationPath = vm.subsubsubcategory.FullyQualifiedName;

            productServices.createProduct(locationPath, vm.newProduct.Name, vm.newProduct.Visible)
            .success(function (data, status, headers, config) {

                if (data.isSuccess) {

                    //we delay showing success in order to allow time for search index to update
                    setTimeout(function () {
                        vm.newProduct.IsSending = false;
                        vm.newProduct.SendingComplete = true;

                        vm.newProduct.Results.IsSuccess = true;
                        vm.newProduct.Results.Message = "Product created!";
                        vm.newProductPath = data.SuccessMessage;

                    }, 1400);

                    //Give searh index time to update (short, medium, long):
                    //setTimeout(function () { vm.getProducts(false) }, 760);
                    setTimeout(function () { vm.getProducts(false) }, 1200);
                    setTimeout(function () { vm.getProducts(false) }, 2700);
                }
                else {
                    vm.newProduct.IsSending = false;
                    vm.newProduct.SendingComplete = true;

                    vm.newProduct.Results.IsSuccess = false;
                    vm.newProduct.Results.Message = data.ErrorMessage;

                    if (data.ErrorCode == "Constraint") {
                        vm.productConstraint = true;
                    }
                }

            })
                .error(function (data, status, headers, config) {

                    vm.newProduct.IsSending = false;
                    vm.newProduct.SendingComplete = true;

                    vm.newProduct.Results.IsSuccess = false;

                    //vm.clearInvitationForm();

                    vm.newProduct.IsSending = false;
                    vm.newProduct.SendingComplete = true;
                    vm.newProduct.Results.Message = "An error occurred while attempting to use the service...";
                })
        }




        vm.makeProductVisible = function (index) {

            vm.products.Results[index].Document.visible = null;

            productServices.updateProductVisibleState(vm.products.Results[index].Document.fullyQualifiedName, vm.products.Results[index].Document.name, true)
            .success(function (data, status, headers, config) {

                if (data.isSuccess) {
                    vm.products.Results[index].Document.visible = true;
                }
                else {
                    vm.products.Results[index].Document.visible = false;
                }
            })
                .error(function (data, status, headers, config) {
                    vm.products.Results[index].Document.visible = false;
                })
        }

        vm.makeProductHidden = function (index) {

            vm.products.Results[index].Document.visible = null;

            productServices.updateProductVisibleState(vm.products.Results[index].Document.fullyQualifiedName, vm.products.Results[index].Document.name, false)
            .success(function (data, status, headers, config) {

                if (data.isSuccess) {
                    vm.products.Results[index].Document.visible = false;
                }
                else {
                    vm.products.Results[index].Document.visible = true;
                }
            })
                .error(function (data, status, headers, config) {
                    vm.products.Results[index].Document.visible = true;
                })
        }

        /*========================================
  
          END PRODUCT METHODS
 
         ========================================*/









        /*==============================================================================================================
  

          CHAINED STARTUP METHODS
          Chanied on startup, unchained when called individually after page load.

 
         ===============================================================================================================*/
        /* ==========================================

               GET SUBSUBSUBCATEGORY

        ==========================================*/

        vm.subsubsubcategory = null;

        function getSubsubsubcategory(chained) {
            //Debug.trace("Getting subsubsubcategory...");

            categoryServices.getSubsubsubcategory(vm.categoryAttribute, vm.subcategoryAttribute, vm.subsubcategoryAttribute, vm.subsubsubcategoryAttribute)
            .success(function (data, status, headers, config) {

                vm.subsubsubcategory = data;
                //Debug.trace("Subsubsubcategory received!");

                if (chained) {
                    vm.getProducts(chained);
                    vm.getImageRecords();
                }

            })
                .error(function (data, status, headers, config) {


                })
        }
        /* ==========================================
              END GET SUBSUBSUBCATEGORY
          ==========================================*/
        /* ==========================================

              GET PRODUCTS

        ==========================================*/

        vm.products;

        vm.getProducts = function (chained) {
            //Debug.trace("Getting products...");


            //Reset--------------
            vm.amountToSkip = 0;


            var locationPath = vm.subsubsubcategory.FullyQualifiedName;

            searchServices.searchProducts('', "locationPath eq '" + locationPath + "'", vm.productSorting.OrderByString, 0, vm.amountPerPage)
            .success(function (data, status, headers, config) {

                vm.products = data;
                vm.reloadingProducts = false;

                vm.amountToSkip = vm.amountToSkip + vm.amountPerPage;
                vm.resultsAvailable = data.Count;

                if (chained) {
                    vm.getAccount();
                }

                //Debug.trace("Products received!");

            })
                .error(function (data, status, headers, config) {


                })
        }



        /* ==========================================
              END GET PRODUCTS
        ==========================================*/

        /* ==========================================

              GET ACCOUNT

        ==========================================*/


        vm.account = null

        vm.getAccount = function () {

            //Debug.trace('Getting account details...');

            accountServices.getAccount()
                    .success(function (data, status, headers, config) {
                        vm.account = data;
                        vm.getProductSortables();
                        // Manage routes for subsubsubscribe / upgrade / card so the correct modal can be initiated
                        //Debug.trace("Route action: " + vm.routeAction);
                    })
                    .error(function (data, status, headers, config) {
                        //
                    })
        }

        /* ==========================================
               END GET ACCOUNT
        ==========================================*/



        /* ==========================================

         REORDERING METHODS

       ==========================================*/


        vm.newOrder = null;
        vm.useAlphabeticalOrdering = false;
        vm.reorderProcessing = false;
        vm.reorderProcessingSuccess = false;
        vm.reorderProcessingFailed = false;
        vm.reorderErrorMessage = "";

        vm.startOrdering = function (reorderType) { //<-- reorderType is not used, but must be present to avoid breaking ProductPanel which uses it when inserted in subs and subsubs

            vm.cancelOrdering();
            vm.newOrder = [];
            vm.reorderType = reorderType;

            //Pipe in values and rename to generic items----------------
            for (var i = 0, len = vm.products.Results.length; i < len; ++i) {
                var item = { ID: vm.products.Results[i].Document.id, Name: vm.products.Results[i].Document.name, Visible: vm.products.Results[i].Document.visible };
                vm.newOrder.push(item);
            }
        }

        vm.cancelOrdering = function () {
            vm.newOrder = null;
            vm.useAlphabeticalOrdering = false;
            vm.reorderProcessing = false;
            vm.reorderProcessingFailed = false;
            vm.reorderProcessingSuccess = false;
            vm.reorderErrorMessage = "";
        }

        vm.toggleAlphabeticalOrdering = function () {
            if (!vm.useAlphabeticalOrdering) {
                vm.useAlphabeticalOrdering = true;
            }
            else {
                vm.useAlphabeticalOrdering = false;
            }
            vm.newOrder.sort(function (a, b) {
                if (a.Name.toLowerCase() < b.Name.toLowerCase()) return -1;
                if (a.Name.toLowerCase() > b.Name.toLowerCase()) return 1;
                return 0;
            })

        }

        vm.saveOrder = function () {

            vm.reorderProcessing = true;


            if (vm.useAlphabeticalOrdering) {

                productServices.resetProductOrder(vm.subsubsubcategory.FullyQualifiedName)
                        .success(function (data, status, headers, config) {
                            vm.reorderProcessing = false;
                            if (data.isSuccess) {
                                vm.reorderProcessingSuccess = true;
                                //Give searh index time to update:
                                //Give searh index time to update (short, medium, long):
                                setTimeout(function () { vm.getProducts(false) }, 640);
                                setTimeout(function () { vm.getProducts(false) }, 1300);
                                setTimeout(function () { vm.getProducts(false) }, 2800);
                            }
                            else {
                                vm.reorderProcessingFailed = true;
                                vm.reorderProcessingSuccess = false;
                                vm.reorderErrorMessage = data.ErrorMessage;
                            }
                        })
                        .error(function (data, status, headers, config) {
                            vm.reorderProcessing = false;
                            vm.reorderProcessingFailed = true;
                            vm.reorderProcessingSuccess = false;
                            vm.reorderErrorMessage = "Could not connect!";
                        })
                }
            else {
                    //Create array of IDs in the new order ----
                    var idList = vm.newOrder.map(function (item) {
                        return item['ID'];
                    });

                productServices.reorderProducts(idList, vm.subsubsubcategory.FullyQualifiedName)
                        .success(function (data, status, headers, config) {
                            
                            if (data.isSuccess) {
                                
                                //Give searh index time to update:                                
                                setTimeout(function () {
                                    vm.reorderProcessing = false;
                                    vm.reorderProcessingSuccess = true;
                                }, 1400);

                                //Give searh index time to update (short, medium, long):
                                //setTimeout(function () { vm.getProducts(false) }, 640);
                                setTimeout(function () { vm.getProducts(false) }, 1200);
                                setTimeout(function () { vm.getProducts(false) }, 2700);
                            }
                            else {
                                vm.reorderProcessing = false;
                                vm.reorderProcessingFailed = true;
                                vm.reorderProcessingSuccess = false;
                                vm.reorderErrorMessage = data.ErrorMessage;
                            }
                        })
                        .error(function (data, status, headers, config) {
                            vm.reorderProcessing = false;
                            vm.reorderProcessingFailed = true;
                            vm.reorderProcessingSuccess = false;
                            vm.reorderErrorMessage = "Could not connect!";
                        })
                }
            
        }

        /* ==========================================

         END REORDERING METHODS

       ==========================================*/















        /* ==========================================
           CURRENT USER PROFILE
       ==========================================*/

        function updateCurrentUserProfile() {

            //Debug.trace("Refreshing user profile...");

            sharedServices.getCurrentUserProfile()
            .success(function (data, status, headers, config) {

                vm.currentUserProfile = data; //Used to determine what is shown in the view based on user Role.
                currentUserRoleIndex = vm.userRoles.indexOf(data.Role) //<-- use ACCOUNT roles, NOT PLATFORM roles!

                if (vm.currentUserProfile.Id == "" || vm.currentUserProfile == null) {
                    //Log user out if empty
                    window.location.replace("/login");
                }

                //Debug.trace("Profile refreshed!");
                //Debug.trace("Role index = " + currentUserRoleIndex);

            })
                .error(function (data, status, headers, config) {


                })

        }

        /* ==========================================
               CONTROLLER ACTIVATION
           ==========================================*/

        activate();

        function activate() {

            // Injected variables from the view (via CoreServices/PlatformSettings)
            //Platform --------------------------------------------
            //vm.TrialDaysHold = CoreServiceSettings_Custodian_TrialHoldDays;
            //vm.CustodianFrequencyDescription = CoreServiceSettings_Custodian_FrequencyDescription;
            //vm.UnverifiedAccountsDaysToHold = CoreServiceSettings_Custodian_UnverifiedAccountsDaysToHold;
            //vm.PlatformWorkerFrequencyDescription = CoreServiceSettings_PlatformWorker_FrequencyDescription;

            //Account Roles (used for the logged in Account user, to check Roles accesability
            vm.userRoles = JSON.parse(CoreServiceSettings_AccountUsers_RolesList);

            vm.getFeatured();


            //For <legal-footer></legal-footer>
            vm.termsLink = termsLink;
            vm.privacyLink = privacyLink;
            vm.acceptableUseLink = acceptableUseLink;
            vm.serviceAgreement = serviceAgreement;
            vm.theCurrentYear = new Date().getFullYear();


            // Load local profile for the platfor user.
            vm.currentUserProfile = JSON.parse(CurrentUserProfile);
            currentUserRoleIndex = vm.userRoles.indexOf(vm.currentUserProfile.Role) //<-- Role will indicate what editing capabilites are available.

            if (vm.currentUserProfile.Id == "") {
                //Log user out if empty
                window.location.replace("/login");
            }

            //Update user profile info in case of role updates
            updateCurrentUserProfile();
            // Refresh the profile every 45 seconds (if Role is updated, new editing capabilites will light up for the user)
            setInterval(function () { updateCurrentUserProfile() }, 320000);


            getSubsubsubcategory(true);

            //Debug.trace('subsubsubcategoryIndexController activation complete');


            //Bool: Checks if the users role is allowed
            vm.checkRole = function (lowestRoleAllowed) {

                var allowedIndex = vm.userRoles.indexOf(String(lowestRoleAllowed)); //<-- use Account roles, NOT Platform roles!

                //Debug.trace("Lowest role allowed: '" + lowestRoleAllowed + "'");
                //Debug.trace("Comparing: User: '" + currentUserRoleIndex + "' Allowed: '" + allowedIndex + "'");

                if (currentUserRoleIndex >= allowedIndex) {
                    //Debug.trace("Allowed!");
                    return true;
                }
                else {
                    //Debug.trace("Not allowed!");
                    return false;
                }
            }

            //Bool: Checks if the users role is below allowed Role
            vm.reverseRole = function (lowestRoleAllowed) {
                var allowedIndex = vm.userRoles.indexOf(String(lowestRoleAllowed)); //<-- use Account roles, NOT Platform roles!
                if (currentUserRoleIndex < allowedIndex) {
                    return true;
                }
                else {
                    return false;
                }
            }
        }

    }

})();

