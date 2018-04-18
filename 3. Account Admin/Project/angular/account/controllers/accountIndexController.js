(function () {
    'use strict';

    var controllerId = 'accountIndexController';

    // Inject into main app module:
    angular.module('app')
        .controller(controllerId, [
            'accountServices',
            'accountUserServices',
            'accountIndexModels',
            'sharedServices',
            'paymentPlanServices',
            'billingServices',
            'imageServices',
            'accountSettingsServices',
            '$routeParams',
             accountIndexController
    ]);

    function accountIndexController(accountServices, accountUserServices, accountIndexModels, sharedServices, paymentPlanServices, billingServices, imageServices, accountSettingsServices, $routeParams) {

        //Instantiate Controller as ViewModel
        var vm = this;

        //Set routing actions: ==================================

        //Assign route params, after controller activation params can influence page state
        vm.routeAction = $routeParams.action;

        vm.pageLoading = true;
        vm.paymentPlans = null;
        vm.paymentFrequencies = null;

        //Default Properties: =============================
        vm.title = 'accountIndexController';
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


        //The Account:
        vm.account = null;
        vm.invitationsLoaded = false;


        //Account User:
        vm.currentUserProfile = null;
        var currentUserRoleIndex = null //<-- used internally to check role access, must be updated when getting or refreshing the user.
        vm.userRoles = []; //<-- used internally to check role access, must be updated when getting or refreshing the user.
        //--------------------------------------------------------------------

        /* ==========================================
             Core Service Properties
        ==========================================*/

        // vm.TrialHoldDays = null; //<----comes from CoreServices (via local feed)
        //vm.CustodianFrequencyDescription = null; //<----comes from CoreServices (via local feed)
        //vm.UnverifiedAccountsDaysToHold = null; //<----comes from CoreServices (via local feed)
        //vm.PlatformWorkerFrequencyDescription = null; //<----comes from CoreServices (via local feed)


        /* ==========================================
               Base Controller Methods
        ==========================================*/

        vm.getAccount = function (showLoader) {

            //Debug.trace('Getting account details...');

            if (showLoader)
            {
                vm.pageLoading = true;
            }
            

            accountServices.getAccount()
                    .success(function (data, status, headers, config) {
                        vm.account = data;
                        vm.updateAccountObject();
                        vm.getAccountCapacity();
                        vm.pageLoading = false;

                    })
                    .error(function (data, status, headers, config) {
                        
                    })
        }

        vm.updateAccountObject = function()
        {

            if (vm.account != '') {
                //Update UTC to local time on the client side:
                vm.account.CreatedDate = new Date(vm.account.CreatedDate.toLocaleString());

                if (vm.account.Verified) {
                    vm.account.VerifiedDate = new Date(vm.account.VerifiedDate.toLocaleString());
                }

                if (vm.account.Provisioned) {
                    vm.account.ProvisionedDate = new Date(vm.account.ProvisionedDate.toLocaleString());
                }


                if (vm.account.Verified == false) {
                    //Set closure date for unverified account:
                    vm.accountClosureDate = addDays(vm.account.CreatedDate, vm.UnverifiedAccountsDaysToHold)
                }

                if (vm.account.TrialEndDate != null) {
                    vm.account.TrialEndDate = new Date(vm.account.TrialEndDate.toLocaleString());

                    //Check if Trial is over

                    var now = new Date();

                    if (vm.account.TrialEndDate <= now) {
                        vm.trialActive = false;
                        vm.accountClosureDate = addDays(vm.account.TrialEndDate, vm.TrialDaysHold)

                        //Check if account is now marked for closure
                        if (vm.accountClosureDate <= now) {
                            vm.markedForClosure = true;
                        }

                    }
                }

                //Check if account is marked for closure
                if (vm.account.Closed) {
                    vm.markedForClosure = true;
                }

                //Check if account is a paid account, if so show the CC panel and load the credit card
                //vm.hasCreditCard = true;
                vm.getCreditCard();

                //vm.pageLoading = false;
            }
            else {
                //vm.pageLoading = true;
            }
        }


        vm.accountCapacity = null

        vm.getAccountCapacity = function () {

            //Debug.trace('Getting account capacity...');

            accountServices.getAccountCapacity()
                    .success(function (data, status, headers, config) {
                        vm.accountCapacity = data;

                        // Manage routes for subscribe / upgrade / card so the correct modal can be initiated
                        //Debug.trace("Route action: " + vm.routeAction);

                    })
                    .error(function (data, status, headers, config) {
                        //
                    })
        }









        /*======================================
              Get / Manage Account Setings
         ======================================*/


        vm.accountSettings = null;

        vm.getAccountSettings = function () {

            accountSettingsServices.getAccountSettings()
            .success(function (data, status, headers, config) {

                vm.accountSettings = data;

            })
            .error(function (data, status, headers, config) {

            })
        }

        
        //ContactInfo

        vm.newContactInfoProcessing = false;
        vm.newContactInfoSuccess = false;
        vm.newContactInfoError = false;
        vm.newContactInfoErrorMessage = false;

        vm.newContactInfo =
        {
            phoneNumber: null,
            email: null,
            address1: null,
            address2: null,
            city: null,
            state: null,
            postalCode: null
        }

        vm.editContactInfo = function()
        {
            vm.newContactInfo.phoneNumber = vm.accountSettings.ContactSettings.ContactInfo.PhoneNumber;
            vm.newContactInfo.email = vm.accountSettings.ContactSettings.ContactInfo.Email;
            vm.newContactInfo.address1 = vm.accountSettings.ContactSettings.ContactInfo.Address1;
            vm.newContactInfo.address2 = vm.accountSettings.ContactSettings.ContactInfo.Address2;
            vm.newContactInfo.city = vm.accountSettings.ContactSettings.ContactInfo.City;
            vm.newContactInfo.state = vm.accountSettings.ContactSettings.ContactInfo.State;
            vm.newContactInfo.postalCode = vm.accountSettings.ContactSettings.ContactInfo.PostalCode;
        }

        vm.cancelEditContactInfo = function()
        {
            setTimeout(function () {

                vm.newContactInfoProcessing = false;
                vm.newContactInfoSuccess = false;
                vm.newContactInfoError = false;
                vm.newContactInfoErrorMessage = false;

                vm.newContactInfo.phoneNumber = null;
                vm.newContactInfo.email = null;
                vm.newContactInfo.address1 = null;
                vm.newContactInfo.address2 = null;
                vm.newContactInfo.city = null;
                vm.newContactInfo.state = null;
                vm.newContactInfo.postalCode = null;

            }, 100);
            
        }

        vm.updateAccountContactInfo = function () {

            vm.newContactInfoProcessing = true;
            vm.newContactInfoSuccess = false;
            vm.newContactInfoError = false;
            vm.newContactInfoErrorMessage = false;

            accountSettingsServices.updateContactInfo(vm.newContactInfo.phoneNumber, vm.newContactInfo.email, vm.newContactInfo.address1, vm.newContactInfo.address2, vm.newContactInfo.city, vm.newContactInfo.state, vm.newContactInfo.postalCode)
            .success(function (data, status, headers, config) {

                vm.newContactInfoProcessing = false;

                if(data.isSuccess)
                {
                    vm.newContactInfoSuccess = true;

                    vm.accountSettings.ContactSettings.ContactInfo.PhoneNumber = vm.newContactInfo.phoneNumber;
                    vm.accountSettings.ContactSettings.ContactInfo.Email = vm.newContactInfo.email;
                    vm.accountSettings.ContactSettings.ContactInfo.Address1 = vm.newContactInfo.address1;
                    vm.accountSettings.ContactSettings.ContactInfo.Address2 = vm.newContactInfo.address2;
                    vm.accountSettings.ContactSettings.ContactInfo.City = vm.newContactInfo.city;
                    vm.accountSettings.ContactSettings.ContactInfo.State = vm.newContactInfo.state;
                    vm.accountSettings.ContactSettings.ContactInfo.PostalCode = vm.newContactInfo.postalCode;
                }
                else {
                    vm.newContactInfoError = true;
                    vm.newContactInfoErrorMessage = data.ErrorMessage;
                }

            })
            .error(function (data, status, headers, config) {
                vm.newContactInfoProcessing = false;
                vm.newContactInfoError = true;
                vm.newContactInfoErrorMessage = "An unknown error has occurred.";
            })
        }








        /* =========================================================================

           USERS PANEL DIRECTIVE METHODS

       ============================================================================*/

        vm.usersPanelLoading = true;



        vm.getUserDetail = function (index, reset) {
           
            if (reset == true) {
                //Debug.trace("Resetting details...");
                vm.userDetail.reset();
                vm.userDeletion.reset();
            }

            vm.userDetail.Index = index;

            //Assign selected row to invitationDetail object ------------------------------------------------
            //vm.userDetail.InvitationKey = vm.invitations[index].InvitationKey;
            vm.userDetail.ID = vm.account.Users[index].Id;
            vm.userDetail.UserName = vm.account.Users[index].UserName;
            vm.userDetail.FullName = vm.account.Users[index].FirstName + " " + vm.account.Users[index].LastName;
            vm.userDetail.FirstName = vm.account.Users[index].FirstName;
            vm.userDetail.LastName = vm.account.Users[index].LastName;
            vm.userDetail.Role = vm.account.Users[index].Role;
            vm.userDetail.Email = vm.account.Users[index].Email;
            vm.userDetail.AccountOwner = vm.account.Users[index].AccountOwner;
            vm.userDetail.CreatedDate = vm.account.Users[index].CreatedDate;
            vm.userDetail.Active = vm.account.Users[index].Active;
            vm.userDetail.Photo = vm.account.Users[index].Photo;

        }

        vm.activateUsersPanel = function () {
            vm.usersPanelLoading = true;
            vm.getUsers();
        }

        vm.getUsers = function () {

            vm.usersPanelLoading = true;

            //Debug.trace("Getting users...");

            accountUserServices.getUsers()
                        .success(function (data, status, headers, config) {
                            //Refresh the user list

                            vm.usersPanelLoading = false;

                            vm.account.Users = data;

                            //Remove platform user (if exists) //Moved to C#
                            //var platformUserIndex = vm.account.Users.indexOf("red");
                            //if (platformUserIndex >= 0) {
                                //vm.account.Users.splice(index, 1);
                            //}


                            //Debug.trace("Users returned.");
                            //Debug.trace("vm.account.Users: " + vm.account.Users);

                            //Refresh detail screen if index != null
                            if (vm.userDetail.Index != null) {
                                //Debug.trace("Updating details for user index: " + vm.userDetail.Index);
                                vm.getUserDetail(vm.userDetail.Index, false);
                            }

                            if (!vm.invitationsLoaded) {
                                vm.activateInvitationsPanel();
                                vm.invitationsLoaded = true;
                            }

                })
                        .error(function (data, status, headers, config) {
                            //
                        })
        }



        /* ==========================================
          USER DETAIL BASIC TAB
       ==========================================*/


        //Unbindable properties for editing basic user state
        var editingUser =
            {
                newFirstName: null,
                newLastName: null,
                newEmail: null,
                newRole: null

            }

        vm.userDetail =
            {
                DefaultTabActive: true,

                Index: null,

                ID: null,
                UserName: null,
                Email: null,
                FirstName: null,
                LastName: null,
                Owner: false,
                FullName: null,
                Role: false,
                Active: false,

                // Ownership ----------

                MakeOwner: function () {
                    this.AccountOwner = false;
                },

                RemoveAsOwner: function () {
                    this.AccountOwner = true;
                },

                UpdateRole: function (role) {
                    this.Role = role;
                },

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
                updateRole: {
                    edit: false,
                    processing: false,
                    complete: false,
                    isSuccess: false,
                    message: null,
                },
                updateOwner: {
                    processing: false,
                    complete: false,
                    isSuccess: false,
                    message: null,
                },
                updateActiveState: {
                    processing: false,
                    complete: false,
                    isSuccess: false,
                    message: null,
                },

                reset: function () {

                    this.DefaultTabActive = true;

                    this.updateName.edit = false;
                    this.updateEmail.edit = false;
                    this.updateRole.edit = false;

                    this.updateName.complete = false;
                    this.updateEmail.complete = false;
                    this.updateRole.complete = false;
                    this.updateOwner.complete = false;
                    this.updateActiveState.complete = false;

                    this.updateName.processing = false;
                    this.updateEmail.processing = false;
                    this.updateRole.processing = false;
                    this.updateActiveState.processing = false;
                }


            }

        vm.editUserName = function () {
            vm.userDetail.updateName.edit = true;
            editingUser.newFirstName = angular.copy(vm.userDetail.FirstName);
            editingUser.newLastName = angular.copy(vm.userDetail.LastName);
        };

        vm.cancelUserName = function () {
            vm.userDetail.reset();
            vm.userDetail.FirstName = editingUser.newFirstName;
            vm.userDetail.LastName = editingUser.newLastName;
            vm.userDetail.FullName = vm.userDetail.FirstName + " " + vm.userDetail.LastName;
        };


        vm.updateUserName = function (userId) {

            vm.userDetail.updateName.edit = false;
            vm.userDetail.updateName.processing = true;

            accountUserServices.updateUserName(userId, vm.userDetail.FirstName, vm.userDetail.LastName)
                .success(function (data, status, headers, config) {

                    vm.userDetail.updateName.processing = false;
                    vm.userDetail.updateName.complete = true;

                    if (data.isSuccess) {

                        vm.userDetail.FullName = vm.userDetail.FirstName + " " + vm.userDetail.LastName;

                        //refresh users
                        vm.getUsers();

                        vm.userDetail.updateName.isSuccess = true;
                        vm.userDetail.updateName.message = "Name updated!";

                    } else {

                        vm.userDetail.updateName.isSuccess = false;
                        vm.userDetail.updateName.message = data.ErrorMessage;
                    }
                })
                .error(function (data, status, headers, config) {
                    //vm.showName = true;
                    vm.userDetail.updateName.processing = false;

                    vm.userDetail.updateName.isSuccess = false;
                    vm.userDetail.updateName.complete = true;
                    vm.userDetail.updateName.message = "An error occured while attempting to use the service!";
                });
        };

        vm.resetUpdateUserNameResult = function () {
            if (!vm.userDetail.updateName.isSuccess) {
                //vm.cancelUserName();
                vm.userDetail.updateName.complete = false;
                vm.userDetail.updateName.edit = true;
            }
            else {
                vm.userDetail.reset();
            }
            //vm.userDetail.updateName.complete = false;
        };


        vm.editUserEmail = function () {
            vm.userDetail.updateEmail.edit = true;
            editingUser.newEmail = angular.copy(vm.userDetail.Email);
        };

        vm.cancelUserEmail = function () {
            vm.userDetail.reset();
            vm.userDetail.Email = editingUser.newEmail;
        };


        vm.updateUserEmail = function (userId) {

            vm.userDetail.updateEmail.edit = false;
            vm.userDetail.updateEmail.processing = true;


            accountUserServices.updateUserEmail(userId, vm.userDetail.Email, vm.userDetail.LastName)
                .success(function (data, status, headers, config) {

                    vm.userDetail.updateEmail.processing = false;
                    vm.userDetail.updateEmail.complete = true;

                    if (data.isSuccess) {

                        //refresh users
                        vm.getUsers();

                        vm.userDetail.updateEmail.isSuccess = true;
                        vm.userDetail.updateEmail.message = "Email updated!";

                    } else {

                        vm.userDetail.updateEmail.isSuccess = false;
                        vm.userDetail.updateEmail.message = data.ErrorMessage;
                    }

                })
                .error(function (data, status, headers, config) {

                    vm.userDetail.updateEmail.processing = false;

                    vm.userDetail.updateEmail.isSuccess = false;
                    vm.userDetail.updateEmail.complete = true;
                    vm.userDetail.updateEmail.message = "An error occured while attempting to use the service!";
                });
        };

        vm.resetUpdateUserEmailResult = function () {
            if (!vm.userDetail.updateEmail.isSuccess) {
                //vm.cancelUserEmail();
                vm.userDetail.updateEmail.complete = false;
                vm.userDetail.updateEmail.edit = true;
            }
            else {
                vm.userDetail.reset();
            }
            //vm.userDetail.updateEmail.complete = false;

        };


        vm.editUserRole = function () {
            vm.userDetail.updateRole.edit = true;
            editingUser.newRole = angular.copy(vm.userDetail.Role);
        };

        vm.cancelUserRole = function () {
            vm.userDetail.reset();
            vm.userDetail.Role = editingUser.newRole;
        };

        vm.updateUserRole = function (userId) {

            vm.userDetail.updateRole.edit = false;
            vm.userDetail.updateRole.processing = true;

            accountUserServices.updateUserRole(userId, vm.userDetail.Role)
                .success(function (data, status, headers, config) {

                    vm.userDetail.updateRole.processing = false;
                    vm.userDetail.updateRole.complete = true;

                    if (data.isSuccess) {

                        //refresh users
                        vm.getUsers();

                        vm.userDetail.updateRole.isSuccess = true;
                        vm.userDetail.updateRole.message = "Role updated!";
                    } else {

                        vm.userDetail.updateRole.isSuccess = false;
                        vm.userDetail.updateRole.message = data.ErrorMessage;
                    }
                })
                .error(function (data, status, headers, config) {

                    vm.userDetail.updateRole.processing = false;
                    vm.userDetail.updateRole.isSuccess = false;
                    vm.userDetail.updateRole.complete = true;
                    vm.userDetail.updateRole.message = "An error occured while attempting to use the service!";
                });
        };

        vm.resetUpdateUserRoleResult = function () {
            if (!vm.userDetail.updateRole.isSuccess) {
                vm.cancelUserRole();
            }
            vm.userDetail.updateRole.complete = false;
        };

        vm.changeUserOwnershipStatus = function (userId) {

            var orgOwnership = vm.userDetail.AccountOwner;
            vm.userDetail.AccountOwner = null;

            accountUserServices.changeUserOwnershipStatus(userId, !orgOwnership)
              .success(function (data, status, headers, config) {


                  if (data.isSuccess) {

                      vm.userDetail.AccountOwner = !orgOwnership;
                      //refresh users
                      //vm.getUsers();
                      vm.account.Users[vm.userDetail.Index].AccountOwner = !orgOwnership;

                      vm.userDetail.updateOwner.isSuccess = true;
                      vm.userDetail.updateOwner.message = "Ownership status updated!";
                  } else {
                      vm.userDetail.updateOwner.complete = true;
                      vm.userDetail.AccountOwner = orgOwnership;
                      vm.userDetail.updateOwner.isSuccess = false;
                      vm.userDetail.updateOwner.message = data.ErrorMessage;
                  }
              })
              .error(function (data, status, headers, config) {
                  vm.userDetail.updateOwner.processing = false;
                  vm.userDetail.AccountOwner = orgOwnership;
                  vm.userDetail.updateOwner.isSuccess = false;
                  vm.userDetail.updateOwner.complete = true;
                  vm.userDetail.updateOwner.message = "An error occured while attempting to use the service!";
              });
        };

        vm.resetUpdateUserOwnerResult = function () {

            vm.userDetail.updateOwner.complete = false;
        };




        vm.changeUserActiveState = function (userId) {

            var orgStatus = vm.userDetail.Active;

            vm.userDetail.Active = null

            vm.userDetail.updateActiveState.processing = true;

            accountUserServices.changeUserActiveState(userId, !orgStatus)
              .success(function (data, status, headers, config) {

                  vm.userDetail.updateActiveState.processing = false;
                  vm.userDetail.updateActiveState.complete = true;

                  if (data.isSuccess) {

                      vm.userDetail.Active = !orgStatus;
                      //refresh users
                      //vm.getUsers();
                      vm.account.Users[vm.userDetail.Index].Active = !orgStatus;

                      vm.userDetail.updateActiveState.isSuccess = true;
                      vm.userDetail.updateActiveState.message = "Active state updated!";
                  } else {
                      vm.userDetail.Active = orgStatus;
                      vm.userDetail.updateActiveState.isSuccess = false;
                      vm.userDetail.updateActiveState.message = data.ErrorMessage;
                  }
              })
              .error(function (data, status, headers, config) {
                  vm.userDetail.updateActiveState.processing = false;
                  vm.userDetail.Active = orgStatus;
                  vm.userDetail.updateActiveState.isSuccess = false;
                  vm.userDetail.updateActiveState.complete = true;
                  vm.userDetail.updateActiveState.message = "An error occured while attempting to use the service!";
              });
        };

        vm.resetUpdateUserActiveStateResult = function () {

            vm.userDetail.updateActiveState.complete = false;
        };

        //private method

        function resetUserDetailModalProperties() {
            vm.userDetail.reset();
            vm.userDeletion.reset();
        }




        /* ==========================================
          USER DETAILS LOG TAB
       ==========================================*/

        vm.setUserDetailLogTab = function () {
            accountServices.getAccountLogByUser(vm.userDetail.ID, 15)
            .success(function (data, status, headers, config) {

                vm.userDetail.Log = data;
            })
            .error(function (data, status, headers, config) {

            })
        }





        /* ==========================================
           USER DETAILS ADVANCED TAB
        ==========================================*/


        vm.setAdvancedUserDetailTab = function () {

            //getAllAccountsForUser();

        }

        /* ==========================================
                   DELETE USER
        ==========================================*/

        vm.userDeletion =
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
            vm.userDeletion.Verify = true;
        }

        vm.cancelDeletion = function () {
            vm.userDeletion.reset();
        }

        vm.deleteUser = function (userId) {

            vm.userDeletion.Verify = false;
            vm.userDeletion.Processing = true;

            //Debug.trace("Deleting user...");

            accountUserServices.deleteUser(userId)
            .success(function (data, status, headers, config) {

                vm.userDeletion.Processing = false;
                vm.userDeletion.Complete = true;

                if (data.isSuccess) {

                    //refresh users
                    vm.getUsers();

                    vm.userDeletion.IsSuccess = true;
                    vm.userDeletion.Message = "This user has been deleted.";
                    vm.userDetail.FullName = null;

                }
                else {

                    vm.userDeletion.IsSuccess = false;
                    vm.userDeletion.Message = data.ErrorMessage;
                }

            })
                .error(function (data, status, headers, config) {

                    vm.userDeletion.Processing = false;
                    vm.userDetailUpdates.IsSuccess = false;
                    vm.userDetailUpdates.Complete = true;
                    vm.userDetailUpdates.Message = "An error occurred while attempting to use the service...";

                })


        }


        /* ==========================================
                   SEND PASSWORD
         ==========================================*/


        vm.userPassword =
                    {
                        Complete: false,
                        Processing: false,
                        IsSuccess: false,
                        Message: null,

                        reset: function () {
                            this.Complete = false,
                            this.IsSuccess = false,
                            this.Message = null,
                            this.Processing = false
                        }
                    }

        vm.resetPasswordSend = function () {
            vm.userPassword.reset();
        }

        vm.sendUpdatePassword = function () {

            vm.userPassword.Processing = true;

            accountUserServices.sendPasswordLink(vm.userDetail.Email)
              .success(function (data, status, headers, config) {
                  vm.userPassword.Processing = false;
                  if (data.isSuccess) {
                      vm.userPassword.IsSuccess = true;
                      vm.userPassword.Message = "Password reset link sent!";
                      vm.userPassword.Complete = true;
                  } else {
                      vm.userPassword.IsSuccess = false;
                      vm.userPassword.Message = data.ErrorMessage;
                      vm.userPassword.Complete = true;
                  }
              })
              .error(function (data, status, headers, config) {
                  vm.userPassword.Processing = false;
                  vm.userPassword.IsSuccess = false;
                  vm.userPassword.Message = "An error occurred while using the service.";
                  vm.userPassword.Complete = true;
              });
        };


        /* ==========================================
           INVITATIONS
          ==========================================*/

        // Invitation Properties --------------------

        vm.invitationsPanelLoading = true;
        vm.invitations = null;


        vm.newInvitation =
            {
                AccountOwner: false,
                Email: null,
                FirstName: null,
                LastName: null,
                Role: null,

                // Ownership ----------

                MakeOwner: function () {
                    this.AccountOwner = false;
                },

                RemoveAsOwner: function () {
                    this.AccountOwner = true;
                },

                // Service Processing --------

                IsSending: false,
                SendingComplete: false,

                Results: {
                    IsSuccess: false,
                    Message: null
                },

                // Form Helpers ---------------

                UpdateRole: function (role) {
                    this.Role = role;
                },

                // Cleanup Routine(s) ----------

                Retry: function () {
                    this.IsSending = false;
                    this.SendingComplete = false;
                },

                Clear: function () {
                   //Debug.trace("Clearing new invitation form data.");

                    this.Email = null;
                    this.FirstName = "";
                    this.LastName = "";
                    this.AccountOwner = false;

                    this.Role = vm.userRoles[0];

                    this.IsSending = false;
                    this.SendingComplete = false;

                    this.Results.IsSuccess = false;
                    this.Results.Message = null;

                }
            }

        vm.invitationDetail =
            {
                Index: null,

                InvitationKey: null,
                FullName: null,
                AccountOwner: false,
                Email: null,
                Role: false

            }

        vm.invitationDetailUpdates =
            {
                Complete: false,

                IsSuccess: false,
                Message: null,

                Type: null, //<-- null, 'send' or 'delete'

                Reset: function () {
                    this.Complete = false,
                    this.IsSuccess = false,
                    this.Message = null,
                    this.Type = null
                }
            }


        // Invitation Methods --------------------

        vm.activateInvitationsPanel = function () {

            vm.invitationsPanelLoading = true;
            vm.getInvitations();

            //Debug.trace('User invitations panel activated')
        }

        vm.getInvitations = function () {

            //Debug.trace('Getting invitations...')

            accountUserServices.getInvitations()
                .success(function (data, status, headers, config) {
                    //Refresh the invitation list
                    vm.invitations = data;
                    vm.invitationsPanelLoading = false;

                })
                .error(function (data, status, headers, config) {
                    vm.invitations = null;
                    vm.invitationsPanelLoading = false;
                })
        }

        vm.userConstraint = false; //<-- True if plan needs an upgrade

        vm.inviteUser = function () {

            vm.userConstraint = false;

            vm.newInvitation.IsSending = true;

            //Debug.trace("Inviting user...");

            accountUserServices.inviteUser(vm.newInvitation.Email, vm.newInvitation.FirstName, vm.newInvitation.LastName, vm.newInvitation.Role, vm.newInvitation.AccountOwner)
            .success(function (data, status, headers, config) {

                vm.newInvitation.IsSending = false;
                vm.newInvitation.SendingComplete = true;

                if (data.isSuccess) {

                    vm.newInvitation.Results.IsSuccess = true;
                    vm.newInvitation.Results.Message = "Invite sent!";
                    vm.getInvitations();
                }
                else {
                    vm.newInvitation.Results.IsSuccess = false;
                    vm.newInvitation.Results.Message = data.ErrorMessage;

                    if(data.ErrorCode == "Constraint")
                    {
                        vm.userConstraint = true;
                    }
                }

            })
                .error(function (data, status, headers, config) {

                    vm.newInvitation.Results.IsSuccess = false;

                    //vm.clearInvitationForm();

                    vm.newInvitation.IsSending = false;
                    vm.newInvitation.SendingComplete = true;
                    vm.newInvitation.Results.Message = "An error occurred while attempting to use the service...";
                })
        }

        vm.deleteInvitation = function (invitationKey) {

            vm.invitationDetailUpdates.Type = 'delete';
            vm.invitationDetailUpdates.Complete = false;

           //Debug.trace("Deleting invite...");

            accountUserServices.deleteInvitation(invitationKey)
            .success(function (data, status, headers, config) {

                if (data.isSuccess) {

                    //refresh invitations
                    vm.getInvitations();

                    vm.invitationDetailUpdates.Type = null;


                    vm.invitationDetailUpdates.IsSuccess = true;
                    vm.invitationDetailUpdates.Complete = true;
                    vm.invitationDetailUpdates.Message = "This invitation has been deleted.";

                }
                else {

                    vm.invitationDetailUpdates.Type = null;

                    vm.invitationDetailUpdates.IsSuccess = false;
                    vm.invitationDetailUpdates.Complete = true;
                    vm.invitationDetailUpdates.Message = data.ErrorMessage;
                }

            })
                .error(function (data, status, headers, config) {

                    vm.invitationDetailUpdates.Type = null;

                    vm.invitationDetailUpdates.IsSuccess = false;
                    vm.invitationDetailUpdates.Complete = true;
                    vm.invitationDetailUpdates.Message = "An error occurred while attempting to use the service...";

                })


        }

        vm.resendInvitation = function (invitationKey) {

            vm.invitationDetailUpdates.Type = 'send';

            vm.invitationDetailUpdates.Complete = false;

            //Debug.trace("Resending invite...");

            accountUserServices.resendInvitation(invitationKey)
            .success(function (data, status, headers, config) {


                if (data.isSuccess) {

                    vm.invitationDetailUpdates.Type = null;


                    vm.invitationDetailUpdates.IsSuccess = true;
                    vm.invitationDetailUpdates.Complete = true;
                    vm.invitationDetailUpdates.Message = "Invite has been re-sent.";

                }
                else {

                    vm.invitationDetailUpdates.Type = null;

                    vm.invitationDetailUpdates.IsSuccess = false;
                    vm.invitationDetailUpdates.Complete = true;
                    vm.invitationDetailUpdates.Message = data.ErrorMessage;
                }

            })
                .error(function (data, status, headers, config) {

                    vm.invitationDetailUpdates.Type = null;

                    vm.invitationDetailUpdates.IsSuccess = false;
                    vm.invitationDetailUpdates.Complete = true;
                    vm.invitationDetailUpdates.Message = "An error occurred while attempting to use the service...";
                })

        }


        vm.getInvitationDetail = function (index) {

            vm.invitationDetailUpdates.Reset();

            vm.invitationDetail.Index = index;

            //Assign selected row to invitationDetail object ------------------------------------------------
            vm.invitationDetail.InvitationKey = vm.invitations[index].InvitationKey;
            vm.invitationDetail.FullName = vm.invitations[index].FirstName + " " + vm.invitations[index].LastName;
            vm.invitationDetail.Role = vm.invitations[index].Role;
            vm.invitationDetail.Email = vm.invitations[index].Email;
            vm.invitationDetail.Owner = vm.invitations[index].Owner;
        }


        /* ==========================================
            END INVITATIONS
        ==========================================*/


        /* ============================================================================

           END USERS PANEL DIRECTIVE METHODS

       ================================================================================*/
















        /* ============================================================
   
            Create Subscription

        ================================================================*/

        vm.subscriptionStep1 = false;
        vm.subscriptionStep2 = false;
        vm.subscriptionStep3 = false;
        vm.subscriptionStepSuccess = false;

        vm.selectedSubscriptionPlan = null;
        //vm.selectedSubscriptionPlanName = "";
        vm.selectedSubscriptionFrequency = null;

        vm.subscriptionCard = {

            name: null,
            number: null,
            cvc: null,
            expMonth: null,
            expYear: null,

            error: false,
            errorMessage:null,

            reset: function () {
                this.name = null;
                this.number = null;
                this.cvc = null;
                this.expMonth = null;
                this.expYear = null;

                this.error = false;
                this.errorMessage = null;
            },

        }

        vm.goToSubscriptionStep1 = function(firstRun)
        {
            vm.subscriptionStep1 = true;
            vm.subscriptionStep2 = false;
            vm.subscriptionStep3 = false;
            vm.subscriptionStepSuccess = false;

            if (firstRun)
            {

                $('.newSubscriptionModal').modal('show');

                vm.subscriptionCard.reset();

                paymentPlanServices.getPlans()
                    .success(function (data, status, headers, config) {
                        //Debug.trace(data);
                        vm.paymentPlans = data;
                        vm.selectedSubscriptionPlan = vm.paymentPlans[1]
                        //vm.selectedSubscriptionPlanName = "Small"


                        paymentPlanServices.getFrequencies()
                            .success(function (data, status, headers, config) {
                                //Debug.trace(data);
                                vm.paymentFrequencies = data;
                                vm.selectedSubscriptionFrequency = vm.paymentFrequencies[1];

                            })
                            .error(function (data, status, headers, config) {

                            })

                    })
                    .error(function (data, status, headers, config) {

                    })
            }
        }

        vm.goToSubscriptionStep2 = function ()
        {
            vm.subscriptionStep1 = false;
            vm.subscriptionStep2 = true;
            vm.subscriptionStep3 = false;
            vm.subscriptionStepSuccess = false;
        }

        vm.goToSubscriptionStep3 = function ()
        {
            vm.subscriptionStep1 = false;
            vm.subscriptionStep2 = false;
            vm.subscriptionStep3 = true;
            vm.subscriptionStepSuccess = false;

            vm.subscriptionCard.error = false;
            vm.subscriptionCard.errorMessage = null;

            //Debug.trace(vm.selectedSubscriptionPlan.PaymentPlanName);
            
            paymentPlanServices.subscribe(vm.selectedSubscriptionPlan.PaymentPlanName, vm.selectedSubscriptionFrequency.PaymentFrequencyMonths, vm.subscriptionCard.name, vm.subscriptionCard.number, vm.subscriptionCard.cvc, vm.subscriptionCard.expMonth, vm.subscriptionCard.expYear)
                .success(function (data, status, headers, config) {
                    
                    if (data.isSuccess)
                    {
                        vm.goToSubscriptionStepSuccess();
                    }
                    else
                    {
                        vm.subscriptionCard.error = true;
                        vm.subscriptionCard.errorMessage = data.ErrorMessage;
                        vm.goToSubscriptionStep2();
                    }

                })
                    .error(function (data, status, headers, config) {
                        vm.subscriptionCard.error = true;
                        vm.subscriptionCard.errorMessage = "There was an issue contacting the service, please try again in a moment.";
                        vm.goToSubscriptionStep2();
                })
        }

        vm.goToSubscriptionStepSuccess = function () {

            vm.subscriptionStep1 = false;
            vm.subscriptionStep2 = false;
            vm.subscriptionStep3 = false;
            vm.subscriptionStepSuccess = true;
        }

        vm.endSubscriptionProcess = function() {
            //refresh account before ending processing
            accountServices.getAccount()
                .success(function (data, status, headers, config) {
                    vm.account = data;
                    vm.updateAccountObject();
                })
                .error(function (data, status, headers, config) {
                    //
                })
        }

        vm.switchSelectedSubscriptionPlan = function(plan)
        {
            vm.selectedSubscriptionPlan = vm.paymentPlans[vm.paymentPlans.indexOf(plan)];
        }

        vm.setSelectedNewSubscriptionFrequency = function(frequency)
        {

            vm.selectedSubscriptionFrequency = vm.paymentFrequencies[vm.paymentFrequencies.indexOf(frequency)];
        }
























        /* ============================================================
   
             Credit Card Management

        ================================================================*/

        //Models, etc...
        vm.creditCardLoading = true;
        vm.creditCardInfo = null;
        //vm.hasCreditCard = true;

        //GET CARD DATA -------------------------------------------
        vm.getCreditCard = function () {

            vm.creditCardLoading = true;
            //Debug.trace("getting card...");

            accountServices.getCardInfo()
                .success(function (data, status, headers, config) {
                    //Debug.trace(data);

                    vm.creditCardLoading = false;

                    if (data.CardDescription != null) {
                        vm.creditCardInfo = data;
                        
                    }
                    

                })
                .error(function (data, status, headers, config) {

                })
        }

        vm.actvateNewCreditCardModal = function()
        {
            vm.newCard.reset();
            $('.newCreditCardModal').modal('show');
            
        }



        /* ----- Adding/Updating Card -----------------*/


        vm.newCard = {

            name: null,
            number: null,
            cvc: null,
            expMonth: null,
            expYear: null,

            error: false,
            errorMessage: null,
            errorMessages: [],

            isSuccess: false,
            processing: false,

            reset: function () {
                this.name = null;
                this.number = null;
                this.cvc = null;
                this.expMonth = null;
                this.expYear = null;

                this.error = false;
                this.errorMessage = null;
                this.errorMessages = [];

                this.isSuccess = false;
                this.processing = false;
            },

        }

        vm.submitNewCreditCard = function()
        {
            vm.newCard.processing = true;

            vm.newCard.isSuccess = false;
            vm.newCard.error = false;
            vm.newCard.errorMessage = null;
            vm.newCard.errorMessages = [];

            accountServices.addUpdateCard(vm.newCard.name, vm.newCard.number, vm.newCard.cvc, vm.newCard.expMonth, vm.newCard.expYear)
                .success(function (data, status, headers, config) {

                    vm.newCard.processing = false;

                    if (data.isSuccess)
                    {
                        vm.newCard.isSuccess = true;
                        vm.getCreditCard();
                    }
                    else
                    {
                        vm.newCard.error = true;
                        vm.newCard.errorMessage = data.ErrorMessage;
                        vm.newCard.errorMessages = data.ErrorMessages;
                    }

                }).error(function (data, status, headers, config) {

                    vm.newCard.processing = false;

                    vm.newCard.error = true;
                    vm.newCard.errorMessage = "There was an issue contacting the service, please try again in a moment.";
                })

        }

























        /* ============================================================
           
           Payment Plan Management

       ================================================================*/


        vm.updatePlanName = null;

        vm.updatePlanSuccess = null;
        vm.updatePlanMessage = null;

        vm.updatePlanProcessing = false;
        vm.updatePlanComplete = false;


        //GET PLANS ----------------------------------------------
        vm.activateManagePlanModal = function () {

            $("#planUpdateSuccessMessage").hide();
            $("#planUpdateErrorMessage").hide();

            vm.updatePlanName = null;

            vm.updatePlanSuccess = null;
            vm.updatePlanMessage = null;

            vm.updatePlanProcessing = false;
            vm.updatePlanComplete = false;

            $('.manageAccountPlanModal').modal('show');

            //Debug.trace("getting plans...");
            paymentPlanServices.getPlans()
                .success(function (data, status, headers, config) {
                    //Debug.trace(data);
                    vm.paymentPlans = data;

                    //Check if the current accounts plan is in this list, if not it may be inviible, get a reference to it and add it to the list:
                    var match = false;

                    for (var i = 0; i < vm.paymentPlans.length; ++i) {
                       //Debug.trace(vm.paymentPlans[i].PaymentPlanName + ", " + vm.account.PaymentPlanName);
                        if (vm.paymentPlans[i].PaymentPlanName == vm.account.PaymentPlanName) {               
                            match = true;
                        }
                    }
                    if(match == false)
                    {
                       //Debug.trace("No matches found, get data for plan individually (" + vm.account.PaymentPlanName + ")");
                        //No matches found, get data for plan individually
                        paymentPlanServices.getPlan(vm.account.PaymentPlanName)
                            .success(function (data, status, headers, config) {
                                //Debug.trace(data);
                                vm.paymentPlans.push(data);
                                //resort the data
                                vm.paymentPlans.sort(function(a, b){
                                    return a.MaxUsers - b.MaxUsers;
                                })

                            })
                            .error(function (data, status, headers, config) {
                                //Debug.trace(data);
                            })
                    }

                })
                .error(function (data, status, headers, config) {

                })


            paymentPlanServices.getFrequencies()
                .success(function (data, status, headers, config) {
                    //Debug.trace(data);
                    vm.paymentFrequencies = data;
                })
                    .error(function (data, status, headers, config) {

                })

        }




        //CHANGE PLANS ------------------------------------

        vm.updateAccountPlan = function(planName)
        {

            vm.cancelFrequency();

            $("#planUpdateSuccessMessage").slideUp();
            $("#planUpdateErrorMessage").slideUp();

            vm.updatePlanName = planName;
            vm.updatePlanSuccess = null;
            vm.updatePlanMessage = null;

            vm.updatePlanProcessing = true;
            vm.updatePlanComplete = false;


            paymentPlanServices.updatePlan(planName, vm.account.PaymentFrequencyMonths)
                .success(function (data, status, headers, config) {

                    vm.updatePlanProcessing = false;
                    vm.updatePlanComplete = true;

                    vm.updatePlanName = null;
                    
                    //Debug.trace(data.isSuccess);

                    if (data.isSuccess) {
                        //vm.activateManagePlanModal();
                                            
                        //refresh account before ending processing
                        accountServices.getAccount()
                            .success(function (data, status, headers, config) {
                                vm.account = data;
                                vm.updateAccountObject();
                                vm.updatePlanSuccess = true;
                                vm.updatePlanMessage = "Your plan has been changed to '" + planName + "'";

                                $("#planUpdateSuccessMessage").slideDown(function () {
                                    setTimeout(
                                      function () {
                                          $("#planUpdateSuccessMessage").slideUp();
                                      }, 3500);
                                });

                            })
                            .error(function (data, status, headers, config) {
                                //
                            })

                        //showNotification("Your plan has been changed to " + planName.toLowerCase(), "Success", null, false);
                    }
                    else
                    {
                        vm.updatePlanSuccess = false;
                        vm.updatePlanMessage = data.ErrorMessage;
                        $("#planUpdateErrorMessage").slideDown(function () {
                            setTimeout(
                                      function () {
                                          $("#planUpdateSuccessMessage").slideUp();
                                      }, 3500);
                        });
                        //showNotification(data.ErrorMessage, "Alert", null, false);
                    }

                })
                .error(function (data, status, headers, config) {

                    vm.updatePlanProcessing = false;
                    vm.updatePlanComplete = true;

                    vm.updatePlanName = null;
                    vm.updatePlanSuccess = false;
                    vm.updatePlanMessage = "An error occured when communicating with the service.";
                    $("#planUpdateErrorMessage").slideDown(function () {
                        setTimeout(
                                      function () {
                                          $("#planUpdateSuccessMessage").slideUp();
                                      }, 3500);
                    });
                    //showNotification("An error occured when communicating with the service.", "Alert", null, false);
                })
        }



        /* =========================================================================================
           End Payment Plan Management
       =============================================================================================*/



        /* ====================================================================================

            Manage Frequency

        ======================================================================================*/


        vm.manageFrequency =
            {

                //Frequency: vm.account.PaymentFrequency,

                UpdateFrequency: function (frequency) {
                    this.Frequency = frequency;
                },


                update: {
                    edit: false,
                    processing: false,
                    complete: false,
                    isSuccess: false,
                    message: null,
                },

                reset: function () {

                    this.Frequency = vm.account.PaymentFrequency,

                    this.update.edit = false;
                    this.update.complete = false;
                    this.update.processing = false;

                }


            }

        vm.editFrequency = function () {
            vm.manageFrequency.update.edit = true;
            vm.manageFrequency.Frequency = vm.account.PaymentFrequency
            //editingUser.newRole = angular.copy(vm.userDetail.Role);
        };

        vm.cancelFrequency = function () {
            vm.manageFrequency.reset();
            //vm.updateFrequency.Frequency = vm.account;
        };

        vm.updateFrequency = function (frequency) {

            vm.manageFrequency.update.edit = false;
            vm.manageFrequency.update.processing = true;

            paymentPlanServices.updatePlan(vm.account.PaymentPlanName, frequency.PaymentFrequencyMonths)
                .success(function (data, status, headers, config) {

                    vm.manageFrequency.update.edit = false;
                    vm.manageFrequency.update.processing = false;
                    vm.manageFrequency.update.complete = true;

                    if (data.isSuccess) {
                        //vm.activateManagePlanModal();


                        //refresh account before ending processing
                        accountServices.getAccount()
                            .success(function (data, status, headers, config) {
                                vm.account = data;
                                vm.updateAccountObject();
                                
                                vm.manageFrequency.update.isSuccess = true;
                                vm.manageFrequency.update.message = "Your payment frequency has been changed to " + frequency.PaymentFrequencyName;

                            })
                            .error(function (data, status, headers, config) {
                                //
                            })

                        //showNotification("Your plan has been changed to " + planName.toLowerCase(), "Success", null, false);
                    }
                    else {
                        vm.manageFrequency.update.isSuccess = false;
                        vm.manageFrequency.update.message = data.ErrorMessage;
                        //showNotification(data.ErrorMessage, "Alert", null, false);
                    }

                })
                .error(function (data, status, headers, config) {

                    vm.manageFrequency.update.isSuccess = false;
                    vm.manageFrequency.update.message = "An error occured when communicating with the service.";

                    vm.manageFrequency.update.edit = false;
                    vm.manageFrequency.update.processing = false;
                    vm.manageFrequency.update.complete = true;

                    //showNotification("An error occured when communicating with the service.", "Alert", null, false);
                })
        };

        vm.resetUpdateFrequencyResult = function () {
            if (!vm.manageFrequency.update.isSuccess) {
                //vm.cancelUserRole();
            }
            vm.manageFrequency.update.complete = false;
        };

       /* =========================================================================================
           End Manage Frequency
       =============================================================================================*/













        /* ==========================================
    
            START BILLINGS PANEL

        ==========================================*/

        vm.activateBillingPanel = function()
        {
            //Get next Invoice if paid account.
            if (vm.account.PaymentPlan.MonthlyRate > 0) {
                vm.getNextInvoice();
                //vm.billingTabActive = true;

                //Load dunning attempts if account is Delinquent
                if (vm.account.Delinquent) {
                    vm.billingSubMenu.update('dunningAttempts')
                }

            }
            else if (vm.account.PaymentPlan.PaymentPlanName == 'Free') {
                //Get charges if free account:
                vm.getPaymentHistory();
                //Else, actvate a different tab
                //vm.billingTabActive = false;
            }
        }

        // - Payment/Charge Variables

        vm.showAccountColumnForPayments = false;
        vm.paymentsPerPage = 5;
        vm.payments_next = [];
        vm.payments_last = [];



        /* =========================================================================

        BILLINGS PANEL SUBMENU

        ============================================================================*/


        vm.billingSubMenu =
            {
                dunningButton: false,
                activeButton: true,
                paymentHistoryButton: false,

                update: function (buttonName) {

                    //Debug.trace(buttonName + " clicked");

                    if (buttonName == 'dunningAttempts') {
                        vm.getDunningAttempts();
                        this.dunningButton = true;
                        this.activeButton = false;
                        this.paymentHistoryButton = false;
                        this.invoiceHistoryButton = false;

                    }
                    if (buttonName == 'active') {
                        this.dunningButton = false;
                        this.activeButton = true;
                        this.paymentHistoryButton = false;
                        this.invoiceHistoryButton = false;

                    }
                    if (buttonName == 'paymentHistory') {
                        vm.getPaymentHistory();
                        this.dunningButton = false;
                        this.activeButton = false;
                        this.paymentHistoryButton = true;
                        this.invoiceHistoryButton = false;


                    }
                },
            }


        /*========================================================

            PAYMENTS/CHARGES

        =========================================================*/


        //---------------------------------------------------

        vm.getPaymentHistory = function () {

            vm.payments = [];
            vm.payments_next = [];
            vm.payments_last = [];

            vm.paymentHistoryPanelLoading = true;

            //Debug.trace("Getting payment transactions (initial list)... ");

            billingServices.getPaymentHistory(vm.paymentsPerPage)
                        .success(function (data, status, headers, config) {

                            vm.paymentHistoryPanelLoading = false;
                            vm.payments = data;

                            //vm.PaymentIDIndex1 = vm.payments[0].PaymentID

                            if (vm.payments.length == vm.paymentsPerPage) {
                                vm.preload_NextPayments();
                            }

                        })
                        .error(function (data, status, headers, config) {
                            vm.paymentHistoryPanelLoading = false;
                        })
        }

        //---------------------------------------------------

        vm.preload_NextPayments = function () {
            vm.paymentsPreloadingNext = true;

            vm.payments_next = [];
            //Debug.trace("Preloading next payments... ");

            billingServices.getPaymentHistory_Next(vm.paymentsPerPage, vm.payments[vm.payments.length - 1].ChargeID)
                        .success(function (data, status, headers, config) {

                            vm.paymentsPreloadingNext = false;
                            vm.payments_next = data;
                        })
                        .error(function (data, status, headers, config) {

                        })


        }

        vm.getPaymentHistory_Next = function () {

            //Debug.trace("Getting next payments... ");

            vm.payments_last = vm.payments;
            vm.payments = vm.payments_next;
            vm.preload_NextPayments();

        }

        //---------------------------------------------------

        vm.preload_LastPayments = function () {
            Transaction
            vm.paymentsPreloadingLast = true;

            vm.payments_last = [];

            //Debug.trace("Preloading last payments... ");

            //Debug.trace(vm.payments);

            billingServices.getPaymentHistory_Last(vm.paymentsPerPage, vm.payments[0].ChargeID)
                        .success(function (data, status, headers, config) {

                            vm.paymentsPreloadingLast = false;
                            vm.payments_last = data;

                        })
                        .error(function (data, status, headers, config) {

                        })


        }

        vm.getPaymentHistory_Last = function () {

            //Debug.trace("Getting last payments... ");

            vm.payments_next = vm.payments;
            vm.payments = vm.payments_last;
            vm.preload_LastPayments();
        }


        //--------------------------------------------------


        /* ==============================================

            PAYMENT/CHARGE DETAILS

            ============================================*/
/*
        vm.loadingPaymentDetails = false;
        //vm.showRefundsTab = true;

        
        vm.getPaymentDetail = function (index, reset) {
            vm.refundPayment.reset();
            vm.showRefundsTab = true;
            //vm.refundPayment.Index = index;

            vm.paymentDetailsDefaultTabActive = true;

            // if (reset == true) {
            //Debug.trace("Resetting details...");
            //vm.paymentDetail.reset();
            //}


            vm.paymentDetail = vm.payments[index];
            vm.paymentDetail.Index = index;

            vm.paymentDetailBalances = null
            vm.loadingPaymentDetails = true;

            billingServices.getBalanceTransactionsForSource(vm.paymentDetail.ChargeID)
                        .success(function (data, status, headers, config) {
                            vm.loadingPaymentDetails = false;
                            vm.paymentDetailBalances = data;

                        })
                        .error(function (data, status, headers, config) {
                            vm.loadingPaymentDetails = false;
                        })

        }
        */



        vm.isNumber = function (n) {
            return !isNaN(parseFloat(n)) && isFinite(n);
        }


        /* ==========================================
            END PAYMENTS
        ==========================================*/


        /* =========================================

            DUNNING ATTEMPTS

        ==========================================*/


        vm.getDunningAttempts = function () {

            vm.dunningAttempts = [];

            vm.dunningAttemptsPanelLoading = true;

            //Debug.trace("Getting dunning attempts... ");

            billingServices.getDunningAttempts()
                        .success(function (data, status, headers, config) {

                            vm.dunningAttemptsPanelLoading = false;
                            vm.dunningAttempts = data;


                        })
                        .error(function (data, status, headers, config) {
                            vm.dunningAttemptsPanelLoading = false;
                        })
        }


        // Next Invoice ----------------------

        vm.invoiceNextPanelLoading = false;

        vm.getNextInvoice = function () {

            vm.invoiceNextPanelLoading = true;

            //Debug.trace("Getting upcoming invoice... ");

            billingServices.getNextInvoice()
                        .success(function (data, status, headers, config) {

                            vm.invoiceNextPanelLoading = false
                            vm.account.NextInvoice = data;

                        })
                        .error(function (data, status, headers, config) {
                            //
                        })
        }


        /* ==========================================
            END DUNNING ATTEMPTS
        ==========================================*/

        /* ==========================================

            END BILLING PANEL

        ==========================================*/









        /* ==========================================

           ACCOUNT CLOSURE PANEL

       ==========================================*/


        vm.accountClosure = {
            defaultState: true,
            warningState: false,
            processingState: false,
            completedState: false,

            isSuccess: false,
            message: '',

            Clear: function () {
                this.defaultState = false;
                this.warningState = false;
                this.processingState = false;
                this.completedState = false;

                this.isSuccess = false;
                this.message = '';
            }


        };

        vm.startAccountClosure = function()
        {
            vm.accountClosure.Clear();
            vm.accountClosure.warningState = true;
        }

        vm.cancelAccountClosure = function () {
            vm.accountClosure.Clear();
            vm.accountClosure.defaultState = true;
        }

        vm.processAccountClosure = function () {

            vm.accountClosure.Clear();
            vm.accountClosure.processingState = true;

            //Debug.trace("Getting upcoming invoice... ");

            accountServices.closeAccount()
                        .success(function (data, status, headers, config) {

                            vm.accountClosure.Clear();
                            vm.accountClosure.completedState = true;

                            if(data.isSuccess)
                            {
                                //vm.accountClosure.isSuccess = true;
                                //vm.accountClosure.message = data.SuccessMessage;
                                vm.getAccount(false);

                                vm.accountInfoTabActive = true;
                                showNotification(data.SuccessMessage, "success", null, false);
                            }
                            else
                            {
                                vm.accountClosure.isSuccess = false;
                                vm.accountClosure.message = data.ErrorMessage;
                            }
                            

                        })
                        .error(function (data, status, headers, config) {

                            vm.accountClosure.Clear();
                            vm.accountClosure.completedState = true;
                            vm.accountClosure.isSuccess = false;
                            vm.accountClosure.message = "An error occured while attempting to use the service.";

                        })


        }

        /* ==========================================

           END ACCOUNT CLOSURE PANEL

       ==========================================*/


















































        /*=====================================================================
        ========================================================================
        ========================================================================

        START IMAGING METHODS
       
        
        ========================================================================*/


        /* ==========================================

          LOAD IMAGES

       ==========================================*/

        vm.imageObjectType = "account"; //<-- Must update on different TYPES
        vm.objectId = null; //<-- Load once and reuse
        vm.imagesReloaded = false;

        vm.imageRecords = null;

        vm.getImageRecords = function () {

            vm.objectId = vm.account.AccountID;

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
                    //Debug.trace("File too large");
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






































 


        /* ==========================================
           CURRENT USER PROFILE
       ==========================================*/

        function updateCurrentUserProfile() {

           //Debug.trace("Refreshing user profile...");

            sharedServices.getCurrentUserProfile()
            .success(function (data, status, headers, config) {

                vm.currentUserProfile = data; //Used to determine what is shown in the view based on user Role.
                currentUserRoleIndex = vm.userRoles.indexOf(data.Role) //<-- use ACCOUNT roles, NOT PLATFORM roles!

                if (vm.currentUserProfile.Id == "" || vm.currentUserProfile == null)
                {
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
            HELPER FUNCTIONS
        ==========================================*/



        //Helper function for calculating when an expired trial account will be closed
        function addDays(date, days) {
            var ms = new Date(date).getTime() + (86400000 * days);
            var added = new Date(ms);
            return added;
        }


        /* ==========================================
               CONTROLLER ACTIVATION
           ==========================================*/

        activate();

        function activate(){

            // Injected variables from the view (via CoreServices/PlatformSettings)
            //Platform --------------------------------------------
            //vm.TrialDaysHold = CoreServiceSettings_Custodian_TrialHoldDays;
            //vm.CustodianFrequencyDescription = CoreServiceSettings_Custodian_FrequencyDescription;
            //vm.UnverifiedAccountsDaysToHold = CoreServiceSettings_Custodian_UnverifiedAccountsDaysToHold;
            //vm.PlatformWorkerFrequencyDescription = CoreServiceSettings_PlatformWorker_FrequencyDescription;

            //Account Roles (used for the logged in Account user, to check Roles accesability
            vm.userRoles = JSON.parse(CoreServiceSettings_AccountUsers_RolesList);

            //CDN URL for User Images
            //vm.cdnUri = JSON.parse(CoreServiceSettings_Urls_AccountImagesCdnUri);

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


            vm.getAccount(true);
            vm.getAccountSettings();

            //Debug.trace('accountIndexController activation complete');

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
        }

    }

})();

