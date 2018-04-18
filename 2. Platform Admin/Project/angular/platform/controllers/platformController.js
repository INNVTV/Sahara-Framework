(function () {
    'use strict';

    var controllerId = 'platformController';

    // Inject into main app module:
    angular.module('app')
        .controller(controllerId, [
            'platformServices',
            'platformIndexModels',
            'sharedServices',
            'logsServices',
            'plansServices',
             platformController
    ]);

    function platformController(platformServices, platformIndexModels, sharedServices, logsServices, plansServices) {

        //Instantiate Controller as ViewModel
        var vm = this;

        //Default Properties: =============================
        vm.title = 'platformIndexController';
        vm.activate = activate;


        vm.platformUserInvitationUrl = null;
        vm.platformUserPasswordClaimUrl = null;

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


        //Platform User (Logged In):
        vm.currentUserProfile = null;
        var currentUserRoleIndex = null //<-- used internally to check role access, must be updated when getting or refreshing the user.
        var platformRoles = []; //<-- used internally to check role access, must be updated when getting or refreshing the user.
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

        vm.showPlatformLoading = true;

        vm.platform =
             {
                 Name: null,
                 Users: [],

                 CustodianFrequencyDescription: null,
                 WorkerFrequencyDescription: null
             }


        /* ==========================================
               Base Controller Methods
        ==========================================*/




        /* =========================================================================

           COMMUNICATION PANEL DIRECTIVE METHODS

       ============================================================================*/




        vm.newNotification = {

            notificationType: null,
            message: null,
            expirationMinutes: 0,
            ownersOnly: true,
            setExpiration: false,

            
            update: {
                processing: null,
                complete: null
            },

            results: {
                isSuccess: null,
                message: null
            },

            reset: function(type) {
                
                this.notificationType = type;
                this.message = "";
                this.expirationMinutes = 0;
                this.ownersOnly = true;
                this.setExpiration = false;

                this.results.isSuccess = null;
                this.results.message = null;

                this.update.processing = false;
                this.update.complete = false;

                vm.currentNotificationFilter = vm.notificationFilters[0];
            },

            retry: function() {
                
            this.update.processing = false;
            this.update.complete = false;
            }
        }

        vm.activateSendNotificationModal = function (notificationType)
        {
            vm.newNotification.reset(notificationType);
        }

        vm.sendNotification = function () {

            vm.newNotification.update.processing = true;

            if (vm.newNotification.setExpiration == false)
            {
                vm.newNotification.expirationMinutes = 0
            }

            sharedServices.sendNotificationToBulkAccounts(
                    vm.newNotification.notificationType,
                    vm.newNotification.message,
                    vm.newNotification.expirationMinutes,
                    vm.newNotification.ownersOnly,
                    vm.currentNotificationFilter.name,
                    vm.currentNotificationFilter.value
                ).success(function(data, status, headers, config) {

                    vm.newNotification.update.processing = false;
                    vm.newNotification.update.complete = true;

                    if (data.isSuccess) {

                        vm.newNotification.results.isSuccess = true;

                        if (vm.newNotification.ownersOnly) {
                            vm.newNotification.results.message = "'" + vm.newNotification.notificationType + "' notification has been sent to all " + vm.currentNotificationFilter.label + " accounts/owners";
                        } else {
                            vm.newNotification.results.message = "'" + vm.newNotification.notificationType + "' notification has been sent to all " + vm.currentNotificationFilter.label + " accounts/users";
                        }
                    } else {
                        vm.newNotification.results.isSuccess = false;
                        vm.newNotification.results.message = data.ErrorMessage;
                    }

                })
                .error(function (data, status, headers, config) {

                    vm.newNotification.update.processing = false;
                    vm.newNotification.update.complete = true;

                    vm.newNotification.results.isSuccess = false;
                    vm.newNotification.results.message = "There was a problem communicating with the service.";

                });
        }



        //---Notification Filters --------------

        vm.currentNotificationFilter = null

        vm.updateCurrentNotificationFilter = function (filter) {
            vm.currentNotificationFilter = filter;

            Debug.trace('Updated notifications filter to: ' + vm.currentNotificationFilter.label);

        }


        // Notification Helpers ----------------


        vm.AddWorkerMinutes = function(expirationMinutes)
        {

            return +expirationMinutes + +vm.platform.WorkerFrequencyMinutes;
        }

        vm.generateNotificationDate1 = function(expirationMinutes)
        {
            var d = new Date();
            d.setMinutes(d.getMinutes() + +expirationMinutes);
            return d;
        }

        vm.generateNotificationDate2 = function(expirationMinutes)
        {
            var d = new Date();
            d.setMinutes(d.getMinutes() + vm.AddWorkerMinutes(expirationMinutes));
            return d;
        }



        /* =========================================================================

           USERS PANEL SUBMENU

       ============================================================================*/


        vm.userSubMenu =
            {
                activeButton: true,
                invitationsButton: false,
                passwordClaimsButton: false,

                update: function (buttonName) {

                    Debug.trace(buttonName + " clicked");

                    if (buttonName == 'active') {
                        vm.getUsers();
                        this.activeButton = true;
                        this.invitationsButton = false;
                        this.passwordClaimsButton = false;

                    }
                    if (buttonName == 'invitations') {
                        vm.activateInvitationsPanel();
                        this.activeButton = false;
                        this.invitationsButton = true;
                        this.passwordClaimsButton = false;

                    }
                    if (buttonName == 'passwordClaims') {
                        vm.activatePasswordClaimsPanel();
                        this.activeButton = false;
                        this.invitationsButton = false;
                        this.passwordClaimsButton = true;

                    }
                },
            }


        







        /* =========================================================================

           USERS PANEL DIRECTIVE METHODS

       ============================================================================*/

        vm.usersPanelLoading = true;



        vm.getUserDetail = function (index, reset) {

            if (reset == true) {
                Debug.trace("Resetting details...");
                vm.userDetail.reset();
                vm.userDeletion.reset();
            }

            vm.userDetail.Index = index;

            //Assign selected row to invitationDetail object ------------------------------------------------
            //vm.userDetail.InvitationKey = vm.invitations[index].InvitationKey;
            vm.userDetail.ID = vm.platform.Users[index].Id;
            vm.userDetail.UserName = vm.platform.Users[index].UserName;
            vm.userDetail.FullName = vm.platform.Users[index].FirstName + " " + vm.platform.Users[index].LastName;
            vm.userDetail.FirstName = vm.platform.Users[index].FirstName;
            vm.userDetail.LastName = vm.platform.Users[index].LastName;
            vm.userDetail.Role = vm.platform.Users[index].Role;
            vm.userDetail.Email = vm.platform.Users[index].Email;
            vm.userDetail.CreatedDate = new Date(vm.platform.Users[index].CreatedDate.toLocaleString());
            //vm.userDetail.CreatedDate = vm.platform.Users[index].CreatedDate;
            vm.userDetail.Active = vm.platform.Users[index].Active;
            vm.userDetail.Photo = vm.platform.Users[index].Photo;

        }

        vm.activateUsersPanel = function()
        {
            vm.usersPanelLoading = true;
            vm.getUsers();
        }

        vm.getUsers = function () {


            Debug.trace("Getting users...");

            platformServices.getUsers()
                        .success(function (data, status, headers, config) {
                            //Refresh the user list

                            vm.usersPanelLoading = false;

                            vm.platform.Users = data;
                            Debug.trace("Users returned.");

                            //Refresh detail screen if index != null
                            if (vm.userDetail.Index != null) {
                                Debug.trace("Updating details for user index: " + vm.userDetail.Index);
                                vm.getUserDetail(vm.userDetail.Index, false);
                            }

                        })
                        .error(function (data, status, headers, config) {
                            //
                        })
        }


        /* ==========================================
            CREATE USER MODAL
        ==========================================*/


        vm.newUser =
            {
                Email: null,
                FirstName: null,
                LastName: null,
                Role: null,
                Password: null,
                PasswordConfirm: null,

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
                    Debug.trace("Clearing new user form data.");

                    this.Email = null;
                    this.FirstName = null;
                    this.LastName = null;
                    this.Password = null;
                    this.PasswordConfirm = null;

                    this.Role = vm.platformRoles[0];

                    this.IsSending = false;
                    this.SendingComplete = false;

                    this.Results.IsSuccess = false;
                    this.Results.Message = null;

                }
            }

        vm.createUser = function () {
            vm.newUser.IsSending = true;

            Debug.trace("Creating user...");

            platformServices.createUser(vm.newUser.Email, vm.newUser.FirstName, vm.newUser.LastName, vm.newUser.Role, vm.newUser.Password, vm.newUser.PasswordConfirm)
            .success(function (data, status, headers, config) {

                vm.newUser.IsSending = false;
                vm.newUser.SendingComplete = true;

                if (data.isSuccess) {

                    vm.newUser.Results.IsSuccess = true;
                    vm.newUser.Results.Message = "User created!";
                    vm.getUsers();
                }
                else {
                    vm.newUser.Results.IsSuccess = false;
                    vm.newUser.Results.Message = data.ErrorMessage;
                }

            })
                .error(function (data, status, headers, config) {

                    vm.newUser.Results.IsSuccess = false;

                    vm.newUser.IsSending = false;
                    vm.newUser.SendingComplete = true;
                    vm.newUser.Results.Message = "An error occurred while attempting to use the service...";
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
                FullName: null,
                Role: false,
                Active: false,

                Log: [],

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

                    this.Log = [];

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

            platformServices.updateUserName(userId, vm.userDetail.FirstName, vm.userDetail.LastName)
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


            platformServices.updateUserEmail(userId, vm.userDetail.Email, vm.userDetail.LastName)
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

            platformServices.updateUserRole(userId, vm.userDetail.Role)
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

            vm.userDetail.updateOwner.processing = true;

            platformServices.changeUserOwnershipStatus(userId, !vm.userDetail.Owner)
              .success(function (data, status, headers, config) {

                  vm.userDetail.updateOwner.processing = false;
                  vm.userDetail.updateOwner.complete = true;

                  if (data.isSuccess) {

                      vm.userDetail.Owner = !vm.userDetail.Owner;
                      //refresh users
                      vm.getUsers();

                      vm.userDetail.updateOwner.isSuccess = true;
                      vm.userDetail.updateOwner.message = "Ownership status updated!";
                  } else {

                      vm.userDetail.updateOwner.isSuccess = false;
                      vm.userDetail.updateOwner.message = data.ErrorMessage;
                  }
              })
              .error(function (data, status, headers, config) {
                  vm.userDetail.updateOwner.processing = false;

                  vm.userDetail.updateOwner.isSuccess = false;
                  vm.userDetail.updateOwner.complete = true;
                  vm.userDetail.updateOwner.message = "An error occured while attempting to use the service!";
              });
        };

        vm.resetUpdateUserOwnerResult = function () {

            vm.userDetail.updateOwner.complete = false;
        };




        vm.changeUserActiveState = function (userId) {

            var orgState = vm.userDetail.Active;
            vm.userDetail.Active = null;


            platformServices.changeUserActiveState(userId, !orgState)
              .success(function (data, status, headers, config) {

                  vm.userDetail.updateActiveState.processing = false;
                  vm.userDetail.updateActiveState.complete = true;

                  if (data.isSuccess) {

                      vm.userDetail.Active = !orgState;
                      vm.platform.Users[vm.userDetail.Index].Active = !orgState
                      //refresh users
                      //vm.getUsers();

                      vm.userDetail.updateActiveState.isSuccess = true;
                      vm.userDetail.updateActiveState.message = "Active state updated!";
                  } else {
                      vm.platform.Users[vm.userDetail.Index].Active = orgState
                      vm.userDetail.updateActiveState.isSuccess = false;
                      vm.userDetail.updateActiveState.message = data.ErrorMessage;
                  }
              })
              .error(function (data, status, headers, config) {
                  vm.platform.Users[vm.userDetail.Index].Active = orgState
                  vm.userDetail.updateActiveState.isSuccess = false;
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
            logsServices.getPlatformLogByUser(vm.userDetail.ID, 15)
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

            Debug.trace("Deleting user...");

            platformServices.deleteUser(userId)
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

            platformServices.sendPasswordLink(vm.userDetail.Email)
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
                Email: null,
                FirstName: null,
                LastName: null,
                Role: null,

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
                    Debug.trace("Clearing new invitation form data.");

                    this.Email = null;
                    this.FirstName = "";
                    this.LastName = "";

                    this.Role = vm.platformRoles[0];

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
            vm.getInvitations()

            Debug.trace('Platform invitations panel activated')
        }

        vm.getInvitations = function () {

            Debug.trace('Getting invitations...')

            platformServices.getInvitations()
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

        vm.inviteUser = function () {
            vm.newInvitation.IsSending = true;

            Debug.trace("Inviting user...");

            platformServices.inviteUser(vm.newInvitation.Email, vm.newInvitation.FirstName, vm.newInvitation.LastName, vm.newInvitation.Role)
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

            Debug.trace("Deleting invite...");

            platformServices.deleteInvitation(invitationKey)
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

            Debug.trace("Resending invite...");

            platformServices.resendInvitation(invitationKey)
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




        /* ==========================================
              PASSWORD CLAIMS
        ==========================================*/

        vm.activatePasswordClaimsPanel = function () {

            vm.passwordClaimsPanelLoading = true;
            vm.getPasswordClaims();

            Debug.trace('Platform password claims panel activated');
        }


        vm.getPasswordClaims = function () {

            Debug.trace('Getting claims...');

            platformServices.getPasswordClaims()
                .success(function (data, status, headers, config) {
                    //Refresh the password claims list
                    vm.passwordClaims = data;
                    vm.passwordClaimsPanelLoading = false;
                })
                .error(function (data, status, headers, config) {
                    vm.passwordClaims = null;
                    vm.passwordClaimsPanelLoading = false;
                });
        }


        /* ==========================================
            END PASSWORD CLAIMS
        ==========================================*/















        /* =========================================================================

            DOCUMENT PARTITIONS PANEL SUBMENU

        ============================================================================*/


        vm.documentPartitionSubMenu =
            {
                freeButton: true,
                sharedButton: false,
                dedicatedButton: false,

                update: function (buttonName) {

                    Debug.trace(buttonName + " clicked");

                    vm.getDocumentPartition(buttonName);

                    if (buttonName == 'Free') {                        
                        this.freeButton = true;
                        this.sharedButton = false;
                        this.dedicatedButton = false;

                    }
                    if (buttonName == 'Shared') {
                        this.freeButton = false;
                        this.sharedButton = true;
                        this.dedicatedButton = false;

                    }
                    if (buttonName == 'Dedicated') {
                        this.freeButton = false;
                        this.sharedButton = false;
                        this.dedicatedButton = true;

                    }
                },
            }



        /* =========================================================================

           DOCUMENT PARTITIONS PANEL DIRECTIVE METHODS

       ============================================================================


        vm.getDocumentPartition = function (documentPartitionTierId) {


            vm.documentPartitionsPanelLoading = true;

            Debug.trace("Getting partitions & tier...");

            platformServices.getDocumentPartitionTier(documentPartitionTierId)
                        .success(function (data, status, headers, config) {

                            vm.currentDocumentPartitionTier = data;
                            Debug.trace("Partition tier returned.");

                            platformServices.getDocumentPartitions(documentPartitionTierId)
                                .success(function (data, status, headers, config) {
                                    //Refresh the user list

                                    vm.documentPartitionsPanelLoading = false;

                                    vm.documentPartitions = data;
                                    
                                    //Convert UTC to local time (Now done on C# Controller)
                                    //for (var i = 0; i < vm.documentPartitions.length; i++) {
                                        //vm.documentPartitions[i].CreateDate = new Date(vm.documentPartitions[i].CreateDate.toLocaleString());
                                        //vm.documentPartitions[i].LastUpdatedDate = new Date(vm.documentPartitions[i].LastUpdatedDate.toLocaleString());
                                    //}

                                    Debug.trace("Partitions returned.");


                                })
                                .error(function (data, status, headers, config) {
                                    //
                                })

                        })
                        .error(function (data, status, headers, config) {
                            //
                        })

        }


*/






        /* =========================================================================

           DOCUMENT PARTITION DETAILS MODDAL METHODS

       ============================================================================*/

        vm.getDocumentPartitionDetail = function (index) {

            vm.documentPartitionDetail.DefaultTabActive = true;
            vm.currentDocumentPartition = vm.documentPartitions[index];

            //Load Collection Properties for ths Partition
            vm.currentDocumentPartitionCollectionProperties = null;
            platformServices.getDocumentPartitionCollectionProperties(vm.currentDocumentPartition.DocumentPartitionID)
            .success(function (data, status, headers, config) {

                vm.currentDocumentPartitionCollectionProperties = data;
                Debug.trace("Partition collection properties returned.");

            })
            .error(function (data, status, headers, config) {
                //
            })

        }




















        /* ==========================================
           SQL PARTITIONS
       ==========================================*/

        vm.loadSqlPartitions = function()
        {
            vm.sqlPartitions = null;
            Debug.trace("Loading sql partitions.");

            platformServices.getSqlPartitions()
                .success(function (data, status, headers, config) {

                    vm.sqlPartitions = data;

                    Debug.trace("SQL partitions returned.");

                })
                .error(function (data, status, headers, config) {
                    //
                })
        }


        vm.getSqlPartitionDetail = function (index) {

            vm.sqlPartitionDetail.DefaultTabActive = true;
            vm.currentSqlPartition = vm.sqlPartitions[index];
        }

        vm.loadSqlPartitionSchemas = function () {
            vm.currentSqlPartitionSchemas = null;
            Debug.trace("Loading sql partition schemas.");

            platformServices.getSqlPartitionSchemas(vm.currentSqlPartition.Name)
                .success(function (data, status, headers, config) {

                    vm.currentSqlPartitionSchemas = data;

                    Debug.trace("SQL partition schemas returned.");

                })
                .error(function (data, status, headers, config) {
                    //
                })
        }


        /* ==========================================
           END SQL PARTITIONS
       ==========================================*/





        /* ==========================================
           SEARCH PARTITIONS
       ==========================================*/


        /*       SEARCH PARTITIONS PANEL SUBMENU       */


        vm.searchPartitionSubMenu =
            {
                freeButton: true,
                basicButton: false,
                standardButton: false,

                update: function (buttonName) {

                    Debug.trace(buttonName + " clicked");

                    vm.getSearchPartition(buttonName);

                    if (buttonName == 'Free') {
                        this.freeButton = true;
                        this.basicButton = false;
                        this.standardButton = false;

                    }
                    if (buttonName == 'Basic') {
                        this.freeButton = false;
                        this.basicButton = true;
                        this.standardButton = false;

                    }
                    if (buttonName == 'Standard') {
                        this.freeButton = false;
                        this.basicButton = false;
                        this.standardButton = true;

                    }
                },
            }




        vm.searchPartitions = null;

        vm.loadSearchPartitions = function () {
            vm.searchPartitions = null;
            Debug.trace("Loading search partitions.");

            platformServices.getSearchPartitions()
                .success(function (data, status, headers, config) {

                    vm.searchPartitions = data;

                    Debug.trace("Search partitions returned.");

                })
                .error(function (data, status, headers, config) {
                    //
                })
        }


        /* ==========================================
           END SEARCH PARTITIONS
       ==========================================*/






       /* ==========================================
           STORAGE PARTITIONS
       ==========================================*/

        vm.storagePartitions = null;

        vm.loadStoragePartitions = function () {
            vm.storagePartitions = null;
            Debug.trace("Loading storage partitions.");

            platformServices.getStoragePartitions()
                .success(function (data, status, headers, config) {

                    vm.storagePartitions = data;

                    Debug.trace("Storage partitions returned.");

                })
                .error(function (data, status, headers, config) {
                    //
                })
        }


        /* ==========================================
           END STORAGE PARTITIONS
       ==========================================*/






        /* ==========================================
            LOAD ACCOUNT
        ==========================================*/


        vm.loadAccountDetailsPage = function (accountNameKey) {

            if (accountNameKey != vm.routeId) {
                //vm.routeId = accountNameKey;
                //resetUserDetailModalProperties();

                var newUrl = '/account/' + accountNameKey;
                window.location.href = newUrl;
                //window.location.replace(newUrl);;

                //vm.getAccount();
            }
        }




        /* ==========================================
            SNAPSHOT
        ==========================================*/

        vm.snapshot = null;


        vm.getSnapshot = function () {

            Debug.trace('Getting snapshot...');

            platformServices.getSnapshot()
                .success(function (data, status, headers, config) {
                    vm.snapshot = data;
                })
                .error(function (data, status, headers, config) {
                    
                });
        }


        /* ==========================================
            SNAPSHOT
        ==========================================*/



        /* ==========================================
           LOG DETAILS
       ==========================================*/
        vm.getLogDetail = function (index, reset) {

            vm.logDetail = vm.snapshot.Errors_Log[index];
            //vm.paymentDetail.Index = index;

        }
        /* ==========================================
           LOG DETAILS
       ==========================================*/



        /* ==========================================
           CURRENT USER/LOGIN PROFILE
       ==========================================*/


        function updateCurrentUserProfile() {


            Debug.trace("Refreshing user profile...");

            sharedServices.getCurrentUserProfile()
            .success(function (data, status, headers, config) {


                vm.currentUserProfile = data; //Used to determine what is shown in the view based on user Role.
                currentUserRoleIndex = vm.platformRoles.indexOf(data.Role) //<-- use PLATFORM roles, NOT ACCOUNT roles! Role will indicate what editing capabilites are available.

                Debug.trace("Profile refreshed!");
                Debug.trace("Role index = " + currentUserRoleIndex);

            })
                .error(function (data, status, headers, config) {


                })

        }



        vm.goToAccount = function (accountId) {
            window.location.href = 'account/' + accountId;
        }


        /* ==========================================
               CONTROLLER ACTIVATION
           ==========================================*/

        activate();

        function activate(){

            // Injected variables from the view (via CoreServices/PlatformSettings)
            //Platform --------------------------------------------
            //vm.TrialDaysHold = CoreServiceSettings_Custodian_TrialHoldDays;
            //vm.UnverifiedAccountsDaysToHold = CoreServiceSettings_Custodian_UnverifiedAccountsDaysToHold;

            vm.platform.Name = CoreServiceSettings_ApplicationName;
            vm.platform.CustodianFrequencyDescription = CoreServiceSettings_Custodian_FrequencyDescription;
            vm.platform.WorkerFrequencyDescription = CoreServiceSettings_PlatformWorker_FrequencyDescription;

            //URLs
            vm.platformUserInvitationUrl = CoreServiceSettings_PlatformUsers_InvitationUrl;
            vm.platformUserPasswordClaimUrl = CoreServiceSettings_PlatformUsers_PasswordClaimUrl;

            //Platform Roles (used for the logged in Platform user, to check Roles accesability
            vm.platformRoles = JSON.parse(CoreServiceSettings_PlatformUsers_RolesList);

            //CDN URL for User Images
            vm.cdnUri = JSON.parse(CoreServiceSettings_Urls_PlatformImagesCdnUri);

            // Load local profile for the platfor user.
            vm.currentUserProfile = JSON.parse(CurrentUserProfile);
            currentUserRoleIndex = vm.platformRoles.indexOf(vm.currentUserProfile.Role) //<-- Role will indicate what editing capabilites are available.
            // Refresh the profile every 45 seconds (if Role is updated, new editing capabilites will light up for the user)
            setInterval(function () { updateCurrentUserProfile(); }, 150000);

            vm.newUser.Role = vm.platformRoles[0];
            vm.newInvitation.Role = vm.platformRoles[0];



            vm.notificationFilters = platformIndexModels.getBulkAccountFilters();
            vm.currentNotificationFilter = vm.notificationFilters[0];

            //Load up all availabble plans and inject into dropdown list filters
            plansServices.getPlans()
                            .success(function (data, status, headers, config) {

                                //Inject Into Filters
                                data.forEach(function (plan) {
                                    //account.CreatedDate = new Date(account.CreatedDate).toLocaleString();
                                    //account.CreatedDate = new Date(account.CreatedDate.toLocaleString());
                                    vm.notificationFilters.push({ label: plan.PaymentPlanName + " Accounts", name: 'Accounts.PaymentPlanName', value: plan.PaymentPlanName })
                                });
                            })
                            .error(function (data, status, headers, config) {
                                //
                            })

            Debug.trace('platformIndexController activation complete');
            vm.showPlatformLoading = false;

            vm.getSnapshot();

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

