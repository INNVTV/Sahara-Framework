(function () {
    'use strict';

    var controllerId = 'profileIndexController';

    // Inject into main app module:
    angular.module('app')
        .controller(controllerId, [
            'profileIndexServices',
            //'scaffoldIndexModels',
            'sharedServices',
             profileIndexController
    ]);

    function profileIndexController(profileIndexServices, sharedServices) { //scaffoldIndexModels, sharedServices) {

        //Instantiate Controller as ViewModel
        var vm = this;

        //Default Properties: =============================
        vm.title = 'profileIndexController';
        vm.activate = activate;

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


        //Platform User:
        vm.currentUserProfile = null;
        var currentUserRoleIndex = null //<-- used internally to check role access, must be updated when getting or refreshing the user.
        var platformRoles = []; //<-- used internally to check role access, must be updated when getting or refreshing the user.
        //--------------------------------------------------------------------

        /* ==========================================
             Core Service Properties
        ==========================================*/







        /* ==========================================

           Check for Photo / Update Photo

       ==========================================*/

        // Grab the files and set them to our variable
        function uploadPhoto() {

            //Fade out/clear all hover & error messages
            $("#profilePhotoOverlay").fadeOut(150);            
            $("#photoUploadErrorText").text("");
            $("#photoUploadErrorMessage").slideUp();

            // Check for the various HTML5 File API support.
            //if (window.File && window.FileReader && window.FileList && window.Blob) {

                // Great! All the File APIs are supported.

                Debug.trace("vm.uploadingPhoto = " + vm.uploadingPhoto);

                var file = document.getElementById('photoUploadInput').files[0];

                Debug.trace("photo ready for upload");
                Debug.trace(file);
                //Debug.trace("source=" + file.src);

                Debug.trace("size=" + file.size);
                Debug.trace("type=" + file.type);
                Debug.trace("result=" + file.result);

                if (file.type != "image/jpeg" && file.type != "image/png" && file.type != "image/gif" && file.type != "image/bmp") { //&& file.type != "image/tiff"   <-- Tiffs in future
                    Debug.trace("Not a supported image format");
                    //vm.uploadingPhoto = false;
                    $("#uploadingPhotoAnimation").fadeOut(100);
                    $("#photoUploadErrorText").text("Please only upload an image of type Jpeg, Png, Gif or Bmp.");
                    $("#photoUploadErrorMessage").slideDown('slow').delay(1900).slideUp('slow');
                }
                else if (file.size > "5000000") // (in Bytes) (5mb) 
                {
                    // *!!* (WCF Settings) MaxSize Must also be updated in Sahara.CoreServices.Host.CreateServiceHost()
                    // *!!* (Local Settings) MaxSize Must also be updated in local Web.Config > Configuration > System.Web > httpRuntime > maxRequestLength="10000" (in KB)                    
                    Debug.trace("File too large");
                    $("#uploadingPhotoAnimation").fadeOut(100);
                    $("#photoUploadErrorText").text("Image size too large. Max allowed is 5mb.");
                    $("#photoUploadErrorMessage").slideDown('slow').delay(1900).slideUp('slow');
                }
                else {

                    //Show Loader
                    $("#uploadingPhotoAnimation").fadeIn(200);

                    profileIndexServices.updateProfilePhoto(file).onreadystatechange = function () {
                        if (this.readyState == 4) { //<--0 = notinitialized, 1 = set up, 2 = sent, 3 = in process, 4 = complete
                            
                            var data = JSON.parse(this.responseText);
                            Debug.trace(data);
                            //vm.uploadingPhoto = false;
                            $("#uploadingPhotoAnimation").fadeOut(350);
                            Debug.trace("vm.uploadingPhoto = " + vm.uploadingPhoto);

                            if (data.isSuccess)
                            {
                                vm.currentUserProfile.Photo = data.SuccessMessage;
                                Debug.trace("Photo = " + vm.currentUserProfile.Photo);
                                
                                //refresh currentUserProfile object as well as local site cookies...
                                updateCurrentUserProfile();
                            }
                            else
                            {
                                //Show Error
                                //vm.photoUploadFail = true;
                                //vm.photoUploadFailMessage = data.ErrorMessage;
                                $("#photoUploadErrorText").text(data.ErrorMessage);
                                $("#photoUploadErrorMessage").slideDown('slow').delay(1900).slideUp('slow');
                            }

                        }
                    }

                    //HTML5 File API
                    /*
                    //Create FileReader
                    var reader = new FileReader();

                    //Assign to onLoad event
                    reader.onload = function (event) {

                        //Debug.trace("fileBytes=" + event.target.result);
                        //var fileBytes = event.target.result //<-- Result of the event is the image byte[]
                        var fileResult = reader.result;
                        //var fileBytes = new Uint8Array(reader.result);
                        //Debug.trace("fileBytes=" + fileBytes);
                        //Debug.trace("fileBytesLength=" + fileBytes.length);


                        //Submit photo to API:
                        profileIndexServices.updateProfilePhoto(fileResult)
                             .success(function (data, status, headers, config) {

                                 vm.uploadingPhoto = false;

                                 if (data.isSuccess) {

                                     vm.currentUserProfile.Photo = "http://" + vm.cdnUri + "/" + data.SuccessMessage + "_128x128.png";

                                 } else {
                                     //Show Error
                                     vm.photoUploadFail = true;
                                     vm.photoUploadFailMessage = data.ErrorMessage;
                                 }

                             })
                             .error(function (data, status, headers, config) {

                                 vm.uploadingPhoto = false;
                                 vm.photoUploadFail = true;
                                 vm.photoUploadFailMessage = "An error occurred during upload, please try again.";

                             });

                    }

                    // when the file is read it triggers the onload event above.
                    //reader.readAsBinaryString(file)
                    reader.readAsArrayBuffer(file) //<-- Switched to "readAsArrayBuffer" for IE

                    //Use "reader.readAsDataURL(file)" to get local URL to display the image for display, cropping with JCrop, Etc....
                    */

                }

            //} else {
                //vm.photoUploadFail = true;
                //vm.photoUploadFailMessage = "Your browser does not support HTML5. Please upgrade.";
            //}      
                        
        }


        //*Helper Methods -----------------------------------------------------------------------------------

        //Add function to photo div to trigger hidden file input when clicked
        vm.initiatePhotoUpdate = function () {

            //Click's upload button on hidden file input
            $("#photoUploadInput:hidden").trigger('click');
            
        }

        // Add events to input to fire when status changes
        document.getElementById("photoUploadInput").addEventListener('change', uploadPhoto);


        //Add function to photo div to trigger overlay state on hover
        $('#profilePhoto').hover(function ()
            {
                $("#profilePhotoOverlay").fadeIn(400);
        }, function ()
        {
            $("#profilePhotoOverlay").fadeOut(600);
        })

        //Check for photo function. Used by GetCurrentProfile() & UploadPhoto, uses default photo if null
        vm.checkPhoto = function () {
            Debug.trace("Checking photo. vm.currentUserProfile.Photo = " + vm.currentUserProfile.Photo);
            if (vm.currentUserProfile.Photo == "" || vm.currentUserProfile.Photo == null) {
                vm.currentUserProfile.Photo = 'images/icons/profile/nophoto'
            }
            else {
                vm.currentUserProfile.Photo = vm.cdnUri + "/userphotos/" + vm.currentUserProfile.Photo;
            }
        }

        /* ==========================================
            End Photo Upload
       ==========================================*/











        /* ==========================================

            EDITING PROFILE

       ==========================================*/


        //Unbindable properties for editing basic user state
        vm.editingProfile =
            {
                newFirstName: null,
                newLastName: null,
                newEmail: null,

                currentPassword: null,
                newPassword: null,
                confirmNewPassword: null,

                updateName: {
                    edit: false,
                    processing: false,
                    complete: false,
                    isSuccess: false,
                    message: null
                },
                updateEmail: {
                    edit: false,
                    processing: false,
                    complete: false,
                    isSuccess: false,
                    message: null
                },
                updatePassword: {
                    edit: false,
                    processing: false,
                    complete: false,
                    isSuccess: false,
                    message: null
                },

                reset: function () {

                    this.updateName.edit = false;
                    this.updateEmail.edit = false;
                    this.updatePassword.edit = false;

                    this.updateName.complete = false;
                    this.updateEmail.complete = false;
                    this.updatePassword.complete = false;


                    this.updateName.processing = false;
                    this.updateEmail.processing = false;
                    this.updatePassword.processing = false;


                    this.newFirstName = null;
                    this.newLastName = null;
                    this.newEmail = null;

                    this.currentPassword = null;
                    this.newPassword = null;
                    this.confirmNewPassword = null;

                }

            }


        vm.editUserName = function () {
            vm.editingProfile.updateName.edit = true;
            vm.editingProfile.newFirstName = angular.copy(vm.currentUserProfile.FirstName);
            vm.editingProfile.newLastName = angular.copy(vm.currentUserProfile.LastName);
        };

        vm.cancelUserName = function () {
            vm.editingProfile.reset();
            //vm.userDetail.FirstName = editingProfile.newFirstName;
            //vm.userDetail.LastName = editingProfile.newLastName;
            //vm.userDetail.FullName = vm.userDetail.FirstName + " " + vm.userDetail.LastName;
        };


        vm.updateProfileName = function () {

            vm.editingProfile.updateName.edit = false;
            vm.editingProfile.updateName.processing = true;

            profileIndexServices.updateProfileName(vm.editingProfile.newFirstName, vm.editingProfile.newLastName)
                .success(function (data, status, headers, config) {

                    vm.editingProfile.updateName.processing = false;
                    vm.editingProfile.updateName.complete = true;

                    if (data.isSuccess) {

                        //vm.userDetail.FullName = vm.userDetail.FirstName + " " + vm.userDetail.LastName;

                        vm.editingProfile.updateName.isSuccess = true;
                        vm.editingProfile.updateName.message = "Name updated!";

                        //Immediatly set the new name
                        vm.currentUserProfile.FirstName = vm.editingProfile.newFirstName;
                        vm.currentUserProfile.LastName = vm.editingProfile.newLastName;

                        //refresh currentUserProfile object as well as local site cookies...
                        updateCurrentUserProfile();

                    } else {

                        vm.editingProfile.updateName.isSuccess = false;
                        vm.editingProfile.updateName.message = data.ErrorMessage;
                    }
                })
                .error(function (data, status, headers, config) {
                    //vm.showName = true;
                    vm.editingProfile.updateName.processing = false;

                    vm.editingProfile.updateName.isSuccess = false;
                    vm.editingProfile.updateName.complete = true;
                    vm.editingProfile.updateName.message = "An error occured while attempting to use the service!";
                });
        };

        vm.resetUpdateUserNameResult = function () {
            if (!vm.editingProfile.updateName.isSuccess) {
                //vm.cancelUserName();
                vm.editingProfile.updateName.complete = false;
                vm.editingProfile.updateName.edit = true;
            }
            else {
                vm.editingProfile.reset();
            }
        };




        // Edit Email ----------------------------



        vm.editUserEmail = function () {
            vm.editingProfile.updateEmail.edit = true;
            vm.editingProfile.newEmail = angular.copy(vm.currentUserProfile.Email);
        };

        vm.cancelUserEmail = function () {
            vm.editingProfile.reset();
            //vm.userDetail.Email = editingProfile.newEmail;
        };


        vm.updateProfileEmail = function () {

            vm.editingProfile.updateEmail.edit = false;
            vm.editingProfile.updateEmail.processing = true;


            profileIndexServices.updateProfileEmail(vm.editingProfile.newEmail)
                .success(function (data, status, headers, config) {

                    vm.editingProfile.updateEmail.processing = false;
                    vm.editingProfile.updateEmail.complete = true;

                    if (data.isSuccess) {

                        vm.editingProfile.updateEmail.isSuccess = true;
                        vm.editingProfile.updateEmail.message = "Email updated!";

                        //immediatly set the new email
                        vm.currentUserProfile.Email = vm.editingProfile.newEmail

                        //refresh currentUserProfile object as well as local site cookies...
                        updateCurrentUserProfile();

                    } else {

                        vm.editingProfile.updateEmail.isSuccess = false;
                        vm.editingProfile.updateEmail.message = data.ErrorMessage;
                    }

                })
                .error(function (data, status, headers, config) {

                    vm.editingProfile.updateEmail.processing = false;

                    vm.editingProfile.updateEmail.isSuccess = false;
                    vm.editingProfile.updateEmail.complete = true;
                    vm.editingProfile.updateEmail.message = "An error occured while attempting to use the service!";
                });
        };

        vm.resetUpdateUserEmailResult = function () {
            if (!vm.editingProfile.updateEmail.isSuccess) {
                //vm.cancelUserEmail();
                vm.editingProfile.updateEmail.complete = false;
                vm.editingProfile.updateEmail.edit = true;
            }
            else {
                vm.editingProfile.reset();
            }

        };





        // Edit Password ----------------------------


        vm.editUserPassword = function () {
            vm.editingProfile.updatePassword.edit = true;
        };

        vm.cancelUserPassword = function () {
            vm.editingProfile.reset();
        };


        vm.updateProfilePassword = function () {

            vm.editingProfile.updatePassword.edit = false;
            vm.editingProfile.updatePassword.processing = true;

            if (vm.editingProfile.currentPassword == null) {
                vm.editingProfile.updatePassword.processing = false;
                vm.editingProfile.updatePassword.isSuccess = false;
                vm.editingProfile.updatePassword.complete = true;
                vm.editingProfile.updatePassword.message = "You must include your current password!";
            } else if (vm.editingProfile.newPassword == null) {
                vm.editingProfile.updatePassword.processing = false;
                vm.editingProfile.updatePassword.isSuccess = false;
                vm.editingProfile.updatePassword.complete = true;
                vm.editingProfile.updatePassword.message = "You must include a new password!";
            } else if (vm.editingProfile.confirmNewPassword == null) {
                vm.editingProfile.updatePassword.processing = false;
                vm.editingProfile.updatePassword.isSuccess = false;
                vm.editingProfile.updatePassword.complete = true;
                vm.editingProfile.updatePassword.message = "You must confirm your new password!";
            } else if (vm.editingProfile.newPassword != vm.editingProfile.confirmNewPassword) {
                vm.editingProfile.updatePassword.processing = false;
                vm.editingProfile.updatePassword.isSuccess = false;
                vm.editingProfile.updatePassword.complete = true;
                vm.editingProfile.updatePassword.message = "New password and confirmation do not match!";
            } else {

                profileIndexServices.updateProfilePassword(vm.editingProfile.currentPassword, vm.editingProfile.newPassword)
                    .success(function (data, status, headers, config) {

                        vm.editingProfile.updatePassword.processing = false;
                        vm.editingProfile.updatePassword.complete = true;

                        if (data.isSuccess) {

                            vm.editingProfile.updatePassword.isSuccess = true;
                            vm.editingProfile.updatePassword.message = "Password changed!";


                        } else {

                            vm.editingProfile.updatePassword.isSuccess = false;
                            vm.editingProfile.updatePassword.message = data.ErrorMessage;
                        }

                    })
                    .error(function (data, status, headers, config) {

                        vm.editingProfile.updatePassword.processing = false;

                        vm.editingProfile.updatePassword.isSuccess = false;
                        vm.editingProfile.updatePassword.complete = true;
                        vm.editingProfile.updatePassword.message = "An error occured while attempting to use the service!";
                    });

            }


        };

        vm.resetUpdateUserPasswordResult = function () {
            if (!vm.editingProfile.updatePassword.isSuccess) {

                vm.editingProfile.updatePassword.complete = false;
                vm.editingProfile.updatePassword.edit = true;
            }
            else {
                vm.editingProfile.reset();
            }

        };










 


        /* ==========================================
           CURRENT USER PROFILE
       ==========================================*/

        function updateCurrentUserProfile() {

            Debug.trace("Refreshing user profile...");

            sharedServices.getCurrentUserProfile()
            .success(function (data, status, headers, config) {

                vm.currentUserProfile = data; //Used to determine what is shown in the view based on user Role.
                currentUserRoleIndex = vm.platformRoles.indexOf(data.Role) //<-- use PLATFORM roles, NOT ACCOUNT roles!

                vm.checkPhoto(); //<-- Check user photo

                Debug.trace("Profile refreshed!");
                Debug.trace("Role index = " + currentUserRoleIndex);

            })
                .error(function (data, status, headers, config) {


                })

        }


        /* ==========================================
               LOGOUT
           ==========================================*/

        vm.logOut = function()
        {
            window.location.href = '/login/logout';
        }

        /* ==========================================
               CONTROLLER ACTIVATION
           ==========================================*/

        activate();

        function activate(){

            // Injected variables from the view (via CoreServices/PlatformSettings)

            //Platform Roles (used for the logged in Platform user, to check Roles accesability
            vm.platformRoles = JSON.parse(CoreServiceSettings_PlatformUsers_RolesList);

            //CDN URL for Platform Images
            vm.cdnUri = JSON.parse(CoreServiceSettings_Urls_PlatformImagesCdnUri);


            // Load local profile for the platfor user.
            vm.currentUserProfile = JSON.parse(CurrentUserProfile);
            currentUserRoleIndex = vm.platformRoles.indexOf(vm.currentUserProfile.Role) //<-- Role will indicate what editing capabilites are available.
            // Refresh the profile every 45 seconds (if Role is updated, new editing capabilites will light up for the user)
            setInterval(function () { updateCurrentUserProfile() }, 150000);

            vm.checkPhoto(); //<-- Check user photo

            Debug.trace('profileIndexController activation complete');


            //Bool: Checks if the users role is allowed
            vm.checkRole = function (lowestRoleAllowed) {

                var allowedIndex = vm.platformRoles.indexOf(String(lowestRoleAllowed)); //<-- use Platform roles, NOT account roles!

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
        }

    }

})();

