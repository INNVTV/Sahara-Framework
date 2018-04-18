(function () {
    'use strict';

    var controllerId = 'accountDetailController';

    // TODO: replace app with your module name
    angular.module('app')
        .controller(controllerId,
        [
            'accountDetailServices',
            'accountDetailModels',
            'sharedServices',
            'commerceServices',
            'paymentServices',
            'invoiceServices',
            'billingServices',
            'platformServices',
            'plansServices',
            '$routeParams',
            accountDetailController
        ]);

    function accountDetailController(accountDetailServices, accountDetailModels, sharedServices, commerceServices, paymentServices, invoiceServices, billingServices, platformServices, plansServices, $routeParams) {//(accountDetailServices, ) {

        //Instantiate Controller as ViewModel
        var vm = this;

        // Default Properties===============================
        vm.activate = activate;
        vm.title = 'accountDetailController';

        vm.closureTabActive = false;
        vm.billingTabActive = true;

        vm.accountUserInvitationUrl = null;
        vm.accountUserPasswordClaimUrl = null;


        // Debug Methods ===================================
        // wrap console.log() within the "Debug" namespace
        //    -->  Logs can be removed during minification
        var Debug = {};
        Debug.trace = function (message) {
            console.log(message);
        };


        // Controller Properties

        //Platform User:
        vm.currentUserProfile = null;
        var currentUserRoleIndex = null //<-- used internally to check role access, must be updated when getting or refreshing the user.
        var platformRoles = []; //<-- used internally to check role access, must be updated when getting or refreshing the user.
        //--------------------------------------------------------------------

        /*
        vm.localizeDateTime = function(t) {
            Debug.trace("converting:" + t);
            var d = new Date(t + " UTC");
            Debug.trace("to:" + d);
            document.write(d.toString());
        }*/

        vm.routeId = $routeParams.id; //$route.current.params.id; 
        vm.showGetAccountLoading = true;
        vm.showGetAccountNull = false;
        vm.account = null;
        vm.invitations = null;

        vm.headerBGColor = "#F5F5F5";

        vm.userRoles = [];
        //vm.selectedRole = null;


        /* ====================================================================================      = = = = = = = = = = = = = = = = =          REMOVE
        vm.sendingUserInvite = false;
        vm.showInvitationResults = false;
        vm.sendInvitationResults = null;
        vm.inviteAsAccountOwner = false;
        *

        /* ==========================================
           Core Service Properties
          ==========================================*/

        vm.CustodianFrequencyDescription = null; //<----comes from CoreServices (via local feed)
        vm.UnverifiedAccountsDaysToHold = null; //<----comes from CoreServices (via local feed)
        vm.PlatformWorkerFrequencyDescription = null; //<----comes from CoreServices (via local feed)

        vm.lastActivityDate = null; //<-- derived from logs (latest log item date) 




        /* ==========================================
           Base Controller Methods
          ==========================================*/

        vm.getAccount = function () {

            Debug.trace('Getting account details...');

            vm.showGetAccountLoading = true;
            vm.showGetAccountNull = false;

            accountDetailServices.getAccount(vm.routeId)
                    .success(function (data, status, headers, config) {

                        vm.account = data;

                        if (vm.account != '' && vm.account != null && vm.account != 'undefined') {

                            vm.currentNotificationUser = vm.account.Users[0];

                           
                            //Update UTC to local time on the client side:
                            vm.account.CreatedDate = new Date(vm.account.CreatedDate.toLocaleString());
                            //vm.account.CreatedDate = vm.account.CreatedDate.addMinutes(vm.account.CreatedDate.getTimezoneOffset());


                            if (vm.account.Provisioned) {
                                vm.account.ProvisionedDate = new Date(vm.account.ProvisionedDate.toLocaleString());
                            }

                            if (vm.account.StripeCardID != null)
                            {
                                Debug.trace("getting card...");
                                //vm.hasCreditCard = true;
                                vm.getCreditCard();
                            }

                            if (vm.account.Verified == false) {
                                //Set closure date for unverified account:
                                vm.accountClosureDate = addDays(vm.account.CreatedDate, vm.UnverifiedAccountsDaysToHold)
                            }

                            //Check if account is marked for closure
                            if (vm.account.Closed) {
                                vm.markedForClosure = true;
                            }

                            if(vm.account.AccountEndDate != null)
                            {
                                vm.closureTabActive = true;
                                vm.billingTabActive = false;

                                vm.getClosureStatus();
                            }

                            //Get next Invoice if paid account.
                            if (vm.account.PaymentPlan.MonthlyRate > 0) {
                                vm.getNextInvoice(vm.account.AccountID);
                                //vm.billingTabActive = true;

                                //Load dunning attempts if account is Delinquent
                                if (vm.account.Delinquent) {
                                    vm.billingSubMenu.update('dunningAttempts')
                                }
                                
                            }                          

                            vm.getLastActivityDate();


                            vm.showGetAccountLoading = false;
                        }
                        else {
                            vm.showGetAccountLoading = false;
                            vm.showGetAccountNull = true;
                        }

                        vm.partitionAvailabilityData = null;
                        vm.upgradePathAvailable = null;

                        if (vm.account.Provisioned == false)
                        {
                            //Check if partitions are available:
                            accountDetailServices.partitionsAvailable(vm.account.PaymentPlan.SearchPlan)
                                .success(function (data, status, headers, config) {
                                    vm.partitionAvailabilityData = data;
                                })
                                .error(function (data, status, headers, config) {
                                    //
                                });
                        }
                        else
                        {
                            //Check if upgrade path is available:
                            plansServices.upgradePathAvailable(vm.account.PaymentPlan.PaymentPlanName)
                                .success(function (data, status, headers, config) {
                                    vm.upgradePathAvailable = data;
                                })
                                .error(function (data, status, headers, config) {
                                    //
                                });
                        }




                    })
                    .error(function (data, status, headers, config) {
                        //
                    })
        }

        vm.loadNewAccount = function(accountNameKey){
            
            if(accountNameKey != vm.routeId)
            {
                //vm.routeId = accountNameKey;
                //resetUserDetailModalProperties();

                var newUrl = '/account/' + accountNameKey;
                window.location.href = newUrl;
                //window.location.replace(newUrl);;

                //vm.getAccount();
            }
        }

       

        vm.refresh = function () {
            //vm.getAccount();
            reg
            vm.showGetAccountLoading = true;

            var newUrl = '/account/' + vm.account.AccountNameKey;
            window.location.href = newUrl;
            //window.location.replace(newUrl);

            
        }

        vm.onUserDetailModalClose = function () {
            resetUserDetailModalProperties();
        };

        vm.updateAccountObject = function () {

            if (vm.account != '') {
                //Update UTC to local time on the client side:
                vm.account.CreatedDate = new Date(vm.account.CreatedDate.toLocaleString());


                if (vm.account.Provisioned) {
                    vm.account.ProvisionedDate = new Date(vm.account.ProvisionedDate.toLocaleString());
                }

                if (vm.account.Verified == false) {
                    //Set closure date for unverified account:
                    vm.accountClosureDate = addDays(vm.account.CreatedDate, vm.UnverifiedAccountsDaysToHold)
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


        /* ==========================================
           LAST ACTIVITY DATE
          ==========================================*/

        vm.lastActivityDateLoading = true;

        vm.getLastActivityDate = function()
        {

            accountDetailServices.getAccountLog(vm.account.AccountID, 1)
            .success(function (data, status, headers, config) {

                vm.lastActivityDateLoading = false

                if (data.length > 0)
                {
                    vm.lastActivityDate = data[0].Timestamp;
                }
                else
                {
                    vm.lastActivityDate = vm.account.CreatedDate;
                }
                               
            })
            .error(function (data, status, headers, config) {
                
            })


            

        }





        /* ==========================================
           ACCOUNT NAME
        ==========================================*/

        vm.AccountNameStatus =
            {
                Updating: false,
                Editing: false,
                SendingComplete: false,

                AccountName: null,

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

                    Debug.trace("Clearing update account name form data.");
                    this.Updating = false;
                    this.Editing = false;
                    this.SendingComplete = false;
                }


            }

        vm.editAccountName = function () {
            Debug.trace("Editing account name...");

            vm.AccountNameStatus.NewAccountName = vm.account.AccountName;

            vm.AccountNameStatus.Editing = true;

        }

        vm.cancelUpdateAccountName = function () {

            vm.AccountNameStatus.Clear();

        }

        vm.updateAccountName = function () {
            Debug.trace("Updating account name...");

            vm.AccountNameStatus.Updating = true;
            vm.AccountNameStatus.Editing = false;

            accountDetailServices.updateAccountName(vm.account.AccountID, vm.AccountNameStatus.NewAccountName)
           .success(function (data, status, headers, config) {

               vm.AccountNameStatus.Updating = false;
               vm.AccountNameStatus.SendingComplete = true;

               if (data.isSuccess) {

                   vm.AccountNameStatus.Results.IsSuccess = true;
                   vm.AccountNameStatus.Results.Message = "Name has been changed.";

                   //Update AccountName and AccountNameKey 
                   vm.account.AccountName = vm.AccountNameStatus.NewAccountName;
                   vm.account.AccountNameKey = data.Results[0];
                   vm.account.AccountSiteUrl = "Reload to update";
               }
               else {
                   vm.AccountNameStatus.Results.IsSuccess = false;
                   vm.AccountNameStatus.Results.Message = data.ErrorMessage;

               }

           }).error(function (data, status, headers, config) {

               vm.AccountNameStatus.IsSuccess = false;
               vm.AccountNameStatus.Results.Message = "An error has occurred while using the service.";
           })

        }

        vm.cancelAccountNameEdit = function () {

            vm.AccountNameStatus.Editing = false;
            vm.AccountNameStatus.Updating = false;
        }

        /* ==========================================
           END ACCOUNT NAME
        ==========================================*/







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
            errorMessage: null,

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

        vm.goToSubscriptionStep1 = function (firstRun) {
            vm.subscriptionStep1 = true;
            vm.subscriptionStep2 = false;
            vm.subscriptionStep3 = false;
            vm.subscriptionStepSuccess = false;

            if (firstRun) {

                vm.subscriptionCard.reset();

                plansServices.getPlans()
                    .success(function (data, status, headers, config) {
                        //Debug.trace(data);
                        vm.paymentPlans = data;
                        vm.selectedSubscriptionPlan = vm.paymentPlans[1]
                        //vm.selectedSubscriptionPlanName = "Small"


                        plansServices.getFrequencies()
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

        vm.goToSubscriptionStep2 = function () {
            vm.subscriptionStep1 = false;
            vm.subscriptionStep2 = true;
            vm.subscriptionStep3 = false;
            vm.subscriptionStepSuccess = false;
        }

        vm.goToSubscriptionStep3 = function () {
            vm.subscriptionStep1 = false;
            vm.subscriptionStep2 = false;
            vm.subscriptionStep3 = true;
            vm.subscriptionStepSuccess = false;

            vm.subscriptionCard.error = false;
            vm.subscriptionCard.errorMessage = null;

            Debug.trace(vm.selectedSubscriptionPlan.PaymentPlanName);

            plansServices.subscribeAccount(vm.account.AccountID, vm.selectedSubscriptionPlan.PaymentPlanName, vm.selectedSubscriptionFrequency.PaymentFrequencyMonths, vm.subscriptionCard.name, vm.subscriptionCard.number, vm.subscriptionCard.cvc, vm.subscriptionCard.expMonth, vm.subscriptionCard.expYear)
                .success(function (data, status, headers, config) {

                    if (data.isSuccess) {
                        vm.goToSubscriptionStepSuccess();
                    }
                    else {
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

        vm.endSubscriptionProcess = function () {

            //refresh account before ending processing           
            accountDetailServices.getAccount(vm.routeId)
                .success(function (data, status, headers, config) {
                    vm.account = data;
                    vm.updateAccountObject();
                    //vm.getAccount();

                    //Check if partitions are available for provisioning
                    if (vm.account.Provisioned == false) {
                        //Check if partitions are available:
                        accountDetailServices.partitionsAvailable(vm.account.PaymentPlan.SearchPlan)
                            .success(function (data, status, headers, config) {
                                vm.partitionAvailabilityData = data;
                            })
                            .error(function (data, status, headers, config) {
                                //
                            });
                    }

                })
                .error(function (data, status, headers, config) {
                    //
                })
        }

        vm.switchSelectedSubscriptionPlan = function (plan) {
            vm.selectedSubscriptionPlan = vm.paymentPlans[vm.paymentPlans.indexOf(plan)];
        }

        vm.setSelectedNewSubscriptionFrequency = function (frequency) {

            vm.selectedSubscriptionFrequency = vm.paymentFrequencies[vm.paymentFrequencies.indexOf(frequency)];
        }




        /* ==========================================

           PROVISIONING

       ==========================================*/

        vm.provisioningStatus = null;
        vm.provisioningStatusError = "";

        vm.provisionAccount = function () {

            vm.provisioningStatus = "sending";

            accountDetailServices.provisionAccount(vm.account.AccountID)
                        .success(function (data, status, headers, config) {
                            if (data.isSuccess) {
                                vm.provisioningStatus = "success";
                            }
                            else {
                                vm.provisioningStatus = "error";
                                vm.provisioningStatusError = data.ErrorMessage;
                            }

                        })
                        .error(function (data, status, headers, config) {
                            vm.provisioningStatus = "error";
                            vm.provisioningStatusError = "Could not send request";
                        })
        }

        /* ==========================================
           END PROVISIONING
       ==========================================*/



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

            //Debug.trace("getting plans...");
            plansServices.getPlans()
                .success(function (data, status, headers, config) {
                    //Debug.trace(data);
                    vm.paymentPlans = data;

                    //Check if the current accounts plan is in this list, if not it may be inviible, get a reference to it and add it to the list:
                    var match = false;

                    for (var i = 0; i < vm.paymentPlans.length; ++i) {
                        Debug.trace(vm.paymentPlans[i].PaymentPlanName + ", " + vm.account.PaymentPlanName);
                        if (vm.paymentPlans[i].PaymentPlanName == vm.account.PaymentPlanName) {
                            match = true;
                        }
                    }
                    if (match == false) {
                        Debug.trace("No matches found, get data for plan individually (" + vm.account.PaymentPlanName + ")");
                        //No matches found, get data for plan individually
                        plansServices.getPlan(vm.account.PaymentPlanName)
                            .success(function (data, status, headers, config) {
                                Debug.trace(data);
                                vm.paymentPlans.push(data);
                                //resort the data
                                vm.paymentPlans.sort(function (a, b) {
                                    return a.MaxUsers - b.MaxUsers;
                                })

                            })
                            .error(function (data, status, headers, config) {
                                Debug.trace(data);
                            })
                    }

                })
                .error(function (data, status, headers, config) {

                })


            plansServices.getFrequencies()
                .success(function (data, status, headers, config) {
                    //Debug.trace(data);
                    vm.paymentFrequencies = data;
                })
                    .error(function (data, status, headers, config) {

                    })

        }




        //CHANGE PLANS ------------------------------------

        vm.updateAccountPlan = function (planName) {

            vm.cancelFrequency();

            $("#planUpdateSuccessMessage").slideUp();
            $("#planUpdateErrorMessage").slideUp();

            vm.updatePlanName = planName;
            vm.updatePlanSuccess = null;
            vm.updatePlanMessage = null;

            vm.updatePlanProcessing = true;
            vm.updatePlanComplete = false;


            plansServices.updateAccountPlan(vm.account.AccountID, planName, vm.account.PaymentFrequencyMonths)
                .success(function (data, status, headers, config) {

                    vm.updatePlanProcessing = false;
                    vm.updatePlanComplete = true;

                    vm.updatePlanName = null;

                    Debug.trace(data.isSuccess);

                    if (data.isSuccess) {
                        //vm.activateManagePlanModal();

                        //refresh account before ending processing
                        accountDetailServices.getAccount(vm.routeId)
                            .success(function (data, status, headers, config) {
                                vm.account = data;
                                vm.updateAccountObject();
                                vm.updatePlanSuccess = true;
                                vm.updatePlanMessage = "Plan has been changed to '" + planName + "'";

                                $("#planUpdateSuccessMessage").slideDown(function () {
                                    setTimeout(
                                      function () {
                                          $("#planUpdateSuccessMessage").slideUp();
                                      }, 3500);
                                });

                                //Check if upgrade path is available:
                                plansServices.upgradePathAvailable(vm.account.PaymentPlan.PaymentPlanName)
                                    .success(function (data, status, headers, config) {
                                        vm.upgradePathAvailable = data;
                                    })
                                    .error(function (data, status, headers, config) {
                                        //
                                    });

                                //vm.getAccount();

                            })
                            .error(function (data, status, headers, config) {
                                //
                            })

                        //showNotification("Plan has been changed to " + planName.toLowerCase(), "Success", null, false);
                    }
                    else {
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

            plansServices.updateAccountPlan(vm.account.AccountID, vm.account.PaymentPlanName, frequency.PaymentFrequencyMonths)
                .success(function (data, status, headers, config) {

                    vm.manageFrequency.update.edit = false;
                    vm.manageFrequency.update.processing = false;
                    vm.manageFrequency.update.complete = true;

                    if (data.isSuccess) {
                        //vm.activateManagePlanModal();


                        //refresh account before ending processing
                        accountDetailServices.getAccount(vm.routeId)
                            .success(function (data, status, headers, config) {
                                vm.account = data;
                                vm.updateAccountObject();

                                vm.manageFrequency.update.isSuccess = true;
                                vm.manageFrequency.update.message = "Payment frequency has been changed to " + frequency.PaymentFrequencyName;

                                //vm.getAccount();

                            })
                            .error(function (data, status, headers, config) {
                                //
                            })

                        //showNotification("Plan has been changed to " + planName.toLowerCase(), "Success", null, false);
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

            accountDetailServices.getCardInfo(vm.account.AccountID)
                .success(function (data, status, headers, config) {
                    //Debug.trace(data);
                    vm.creditCardInfo = data;
                    vm.creditCardLoading = false;


                })
                .error(function (data, status, headers, config) {

                })
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

        vm.submitNewCreditCard = function () {
            vm.newCard.processing = true;

            vm.newCard.isSuccess = false;
            vm.newCard.error = false;
            vm.newCard.errorMessage = null;
            vm.newCard.errorMessages = [];

            accountDetailServices.addUpdateCard(vm.account.AccountID, vm.newCard.name, vm.newCard.number, vm.newCard.cvc, vm.newCard.expMonth, vm.newCard.expYear)
                .success(function (data, status, headers, config) {

                    vm.newCard.processing = false;

                    if (data.isSuccess) {
                        vm.newCard.isSuccess = true;
                        vm.getCreditCard();
                    }
                    else {
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

        vm.actvateNewCreditCardModal = function () {
            vm.newCard.reset();
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
                        vm.getUsers(vm.account.AccountID);
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










        /* ==========================================
           USERS
       ==========================================*/

        // User Properties --------------------

        vm.usersPanelLoading = false;


        /* ==========================================
            CREATE USER MODAL
        ==========================================*/


        vm.newUser =
            {
                AccountOwner: false,
                Email: null,
                FirstName: null,
                LastName: null,
                Role: vm.userRoles[0],
                Password: null,
                PasswordConfirm: null,

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
                    Debug.trace("Clearing new user form data.");

                    this.Email = null;
                    this.FirstName = null;
                    this.LastName = null;
                    this.AccountOwner = false;
                    this.Password = null;
                    this.PasswordConfirm = null;

                    this.Role = vm.userRoles[0];

                    this.IsSending = false;
                    this.SendingComplete = false;

                    this.Results.IsSuccess = false;
                    this.Results.Message = null;

                }
            }

            vm.createUser = function () {
                vm.newUser.IsSending = true;

                Debug.trace("Creating user...");

                accountDetailServices.createUser(vm.account.AccountID, vm.newUser.Email, vm.newUser.FirstName, vm.newUser.LastName, vm.newUser.Role, vm.newUser.AccountOwner, vm.newUser.Password, vm.newUser.PasswordConfirm)
                .success(function (data, status, headers, config) {

                    vm.newUser.IsSending = false;
                    vm.newUser.SendingComplete = true;

                    if (data.isSuccess) {

                        vm.newUser.Results.IsSuccess = true;
                        vm.newUser.Results.Message = "User created!";
                        vm.getUsers(vm.account.AccountID);
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
                Owner: false,
                Role: false,
                Photo: null,
                Active: false,

                Notifications: [],

                Log: [],

                OtherAccountsMember: [],
                OtherAccountsOwner: [],

                UpdateRole: function (role) {
                    this.Role = role;
                },

                updateName: {
                    edit:false,
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
                    this.Notifications = [],
                    this.Log = [],
                    this.OtherAccountsMember = [],
                    this.OtherAccountsOwner = [],

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
                },


                //----Notifications Submenu

                notificationsSubMenu: {

                    currentType: null, //<--used for date columns in notificatios grid

                    unreadButton: true,
                    readButton: false,
                    expiredUnreadButton: false,
                    expiredReadButton: false,


                    reset: function()
                    {
                        this.unreadButton = true;
                        this.currentType = "unread";
                        this.readButton = false;
                        this.expiredUnreadButton = false;
                        this.expiredReadButton = false;
                    },

                    update: function (buttonName) {

                        Debug.trace(buttonName + " clicked");

                        if (buttonName == 'unread') {
                            
                            getNotificationsForUser('Unread');

                            this.unreadButton = true;
                            this.readButton = false;
                            this.expiredUnreadButton = false;
                            this.expiredReadButton = false;

                            this.currentType = "unread";

                        }
                        if (buttonName == 'read') {
                            

                            getNotificationsForUser('Read');
                            
                            this.unreadButton = false;
                            this.readButton = true;
                            this.expiredUnreadButton = false;
                            this.expiredReadButton = false;

                            this.currentType = "read";

                        }
                        if (buttonName == 'expiredUnread') {
                            

                            getNotificationsForUser('ExpiredUnread');

                            this.unreadButton = false;
                            this.readButton = false;
                            this.expiredUnreadButton = true;
                            this.expiredReadButton = false;

                            this.currentType = "expired";

                        }
                        if (buttonName == 'expiredRead') {
                            

                            getNotificationsForUser('ExpiredRead');

                            this.unreadButton = false;
                            this.readButton = false;
                            this.expiredUnreadButton = false;
                            this.expiredReadButton = true;

                            this.currentType = "expired";
                        }
                    },
                    }

               
            }



        vm.getUserDetail = function (index, reset) {

            if(reset == true)
            {
                Debug.trace("Resetting details...");
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
            vm.userDetail.CreatedDate = new Date(vm.account.Users[index].CreatedDate.toLocaleString());
            //vm.userDetail.CreatedDate = vm.account.Users[index].CreatedDate;
            vm.userDetail.Active = vm.account.Users[index].Active;
            vm.userDetail.Photo = vm.account.Users[index].Photo;

        }

        /*
        vm.getUser = function (accountId) {
            accountDetailServices.getUsers(accountId)
                        .success(function (data, status, headers, config) {
                            //Refresh the user list
                            vm.account.Users = data;


                        })
                        .error(function (data, status, headers, config) {
                            //
                        })
        }*/

        vm.getUsers = function (accountId) {
            accountDetailServices.getUsers(accountId)
                        .success(function (data, status, headers, config) {
                            //Refresh the user list

                            vm.account.Users = data;

                            vm.currentNotificationUser = vm.account.Users[0];

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

            accountDetailServices.updateUserName(vm.account.AccountID, userId, vm.userDetail.FirstName, vm.userDetail.LastName)
                .success(function (data, status, headers, config) {

                    vm.userDetail.updateName.processing = false;
                    vm.userDetail.updateName.complete = true;

                    if (data.isSuccess) {

                        vm.userDetail.FullName = vm.userDetail.FirstName + " " + vm.userDetail.LastName;
                        
                        //refresh users
                        vm.getUsers(vm.account.AccountID);

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
            else
            {
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
            

            accountDetailServices.updateUserEmail(vm.account.AccountID, userId, vm.userDetail.Email, vm.userDetail.LastName)
                .success(function (data, status, headers, config) {

                    vm.userDetail.updateEmail.processing = false;
                    vm.userDetail.updateEmail.complete = true;

                    if (data.isSuccess) {
                       
                        //refresh users
                        vm.getUsers(vm.account.AccountID);

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
            else
            {
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

            accountDetailServices.updateUserRole(vm.account.AccountID, userId, vm.userDetail.Role)
                .success(function (data, status, headers, config) {

                    vm.userDetail.updateRole.processing = false;
                    vm.userDetail.updateRole.complete = true;

                    if (data.isSuccess) {

                        //refresh users
                        vm.getUsers(vm.account.AccountID);

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

            var orgState = vm.userDetail.AccountOwner
            vm.userDetail.AccountOwner = null;

            vm.userDetail.updateOwner.processing = true;

            accountDetailServices.changeUserOwnershipStatus(vm.account.AccountID, userId, !orgState)
              .success(function (data, status, headers, config) {
                
                  vm.userDetail.updateOwner.processing = false;
                  vm.userDetail.updateOwner.complete = true;

                  if (data.isSuccess) {
                      vm.userDetail.AccountOwner = !orgState
                      //refresh users
                      vm.getUsers(vm.account.AccountID);

                      vm.userDetail.updateOwner.isSuccess = true;
                      vm.userDetail.updateOwner.message = "Ownership status updated!";
                  } else {
                      vm.userDetail.AccountOwner = orgState
                      vm.userDetail.updateOwner.isSuccess = false;
                      vm.userDetail.updateOwner.message = data.ErrorMessage;
                  }
              })
              .error(function (data, status, headers, config) {
                  vm.userDetail.AccountOwner = orgState
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
            vm.userDetail.Active = null

            vm.userDetail.updateActiveState.processing = true;

            accountDetailServices.changeUserActiveState(vm.account.AccountID, userId, !orgState)
              .success(function (data, status, headers, config) {
                  
                  vm.userDetail.updateActiveState.processing = false;
                  vm.userDetail.updateActiveState.complete = true;

                  if (data.isSuccess) {
                      vm.userDetail.Active = !orgState;
                      //refresh users
                      vm.getUsers(vm.account.AccountID);

                      vm.userDetail.updateActiveState.isSuccess = true;
                      vm.userDetail.updateActiveState.message = "Active state updated!";
                  } else {
                      vm.userDetail.Active = orgState;
                      vm.userDetail.updateActiveState.isSuccess = false;
                      vm.userDetail.updateActiveState.message = data.ErrorMessage;
                  }
              })
              .error(function (data, status, headers, config) {
                  vm.userDetail.Active = orgState;
                  vm.userDetail.updateActiveState.processing = false;

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
           USER DETAILS NOTIFICATION TAB
        ==========================================*/


        vm.setNotificationUserDetailTab = function () {

            getNotificationsForUser('Unread');
            vm.userDetail.notificationsSubMenu.reset();

        }

        function getNotificationsForUser(notificationStatus) {

            vm.gettingNotificationsForUser = true;
            vm.userDetail.Notifications = [],

            Debug.trace("Getting notifications for user...");

            sharedServices.getNotifications(notificationStatus, vm.userDetail.ID)
            .success(function (data, status, headers, config) {

                vm.userDetail.Notifications = data

                vm.gettingNotificationsForUser = false;

            }).error(function (data, status, headers, config) {
                vm.gettingNotificationsForUser = false
            })
        }



        /* ==========================================
          USER DETAILS LOG TAB
       ==========================================*/

        vm.setUserDetailLogTab = function()
        {
            accountDetailServices.getAccountLogByUser(vm.account.AccountID, vm.userDetail.ID, 15)
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
            
            getAllAccountsForUser();

        }


        function getAllAccountsForUser(){
            
            vm.gettingOtherAccountsForUser = true;
            vm.userDetail.OtherAccountsMember = [],
            vm.userDetail.OtherAccountsOwner = [],

            Debug.trace("Getting all accounts for user...");

            accountDetailServices.getAllAccountsForEmail(vm.userDetail.Email)
            .success(function (data, status, headers, config) {

                //vm.userDetail.OtherAccounts = data
                
                // seperate into members/owners
                data.forEach(function (userAccount) {
                    if(userAccount.AccountNameKey != vm.account.AccountNameKey){
                        if (userAccount.AccountOwner) {
                            vm.userDetail.OtherAccountsOwner.push(userAccount)
                        }
                        else {
                            vm.userDetail.OtherAccountsMember.push(userAccount)
                        }
                    }
                    
                });

                

                vm.gettingOtherAccountsForUser = false;

            }).error(function (data, status, headers, config) {
                vm.gettingOtherAccountsForUser = false
            })
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
        
        vm.startDeletion = function()
        {
            vm.userDeletion.Verify = true;
        }

        vm.cancelDeletion = function()
        {
            vm.userDeletion.reset();
        }

        vm.deleteUser = function (userId) {

            vm.userDeletion.Verify = false;
            vm.userDeletion.Processing = true;

            Debug.trace("Deleting user...");

            accountDetailServices.deleteUser(vm.account.AccountID, userId)
            .success(function (data, status, headers, config) {

                vm.userDeletion.Processing = false;
                vm.userDeletion.Complete = true;

                if (data.isSuccess) {

                    //refresh users
                    vm.getUsers(vm.account.AccountID);

                    vm.userDeletion.IsSuccess = true;                   
                    vm.userDeletion.Message = "This user has been deleted.";

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

        vm.resetPasswordSend = function()
        {
            vm.userPassword.reset();
        }

        vm.sendUpdatePassword = function () {

            vm.userPassword.Processing = true;

            accountDetailServices.sendPasswordLink(vm.account.AccountID, vm.userDetail.Email)
              .success(function (data, status, headers, config) {
                  vm.userPassword.Processing = false;
                  if(data.isSuccess){
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
           END USERS
       ==========================================*/





















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
                Role: vm.userRoles[0],


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
                    Debug.trace("Clearing new invitation form data.");

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
            vm.getInvitations(vm.account.AccountID)

            Debug.trace('Account invitations panel activated');
        }

        vm.getInvitations = function (accountId) {

            Debug.trace('Getting invitations...')

            if (vm.account.Provisioned)
            {
                accountDetailServices.getInvitations(accountId)
                    .success(function(data, status, headers, config) {
                        //Refresh the invitation list
                        vm.invitations = data;
                        vm.invitationsPanelLoading = false;

                        vm.activatePasswordClaimsPanel();
                    })
                    .error(function(data, status, headers, config) {
                        vm.invitations = null;
                        vm.invitationsPanelLoading = false;
                    });
            }

        }

        vm.inviteUser = function () {
            vm.newInvitation.IsSending = true;

            Debug.trace("Inviting user...");

            accountDetailServices.inviteUser(vm.account.AccountID, vm.newInvitation.Email, vm.newInvitation.FirstName, vm.newInvitation.LastName, vm.newInvitation.Role, vm.newInvitation.AccountOwner)
            .success(function (data, status, headers, config) {

                vm.newInvitation.IsSending = false;
                vm.newInvitation.SendingComplete = true;

                if (data.isSuccess) {

                    vm.newInvitation.Results.IsSuccess = true;
                    vm.newInvitation.Results.Message = "Invite sent!";
                    vm.getInvitations(vm.account.AccountID);
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

            accountDetailServices.deleteInvitation(vm.account.AccountID, invitationKey)
            .success(function (data, status, headers, config) {

                if (data.isSuccess) {

                    //refresh invitations
                    vm.getInvitations(vm.account.AccountID);

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

            accountDetailServices.resendInvitation(vm.account.AccountID, invitationKey)
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









        /* ==========================================
              PASSWORD CLAIMS
        ==========================================*/

        vm.activatePasswordClaimsPanel = function () {

            vm.passwordClaimsPanelLoading = true;
            vm.getPasswordClaims(vm.account.AccountID);

            Debug.trace('Account password claims panel activated');
        }

        vm.getPasswordClaims = function (accountId) {

            Debug.trace('Getting claims...');

            accountDetailServices.getPasswordClaims(accountId)
                .success(function(data, status, headers, config) {
                    //Refresh the password claims list
                    vm.passwordClaims = data;
                    vm.passwordClaimsPanelLoading = false;
                })
                .error(function(data, status, headers, config) {
                    vm.passwordClaims = null;
                    vm.passwordClaimsPanelLoading = false;
                });
        }


        /* ==========================================
            END PASSWORD CLAIMS
        ==========================================*/







        /* ==========================================

            START APPLICATION DATA / ACCOUNT CAPACITY PANEL

        ==========================================*/

        vm.activateApplicationDataPanel = function () {

            vm.accountCapacity = null

            Debug.trace('Getting account capacity...');

            accountDetailServices.getAccountCapacity(vm.account.AccountID)
                    .success(function (data, status, headers, config) {
                        vm.accountCapacity = data;

                        // Manage routes for subscribe / upgrade / card so the correct modal can be initiated
                        Debug.trace("Route action: " + vm.routeAction);

                    })
                    .error(function (data, status, headers, config) {
                        //
                    })
        }



















        /* ==========================================
    
            START BILLINGS PANEL

        ==========================================*/

        // - Payment/Charge Variables

        vm.showAccountColumnForPayments = false;
        vm.paymentsPerPage = 5;
        vm.payments_next = [];
        vm.payments_last = [];
        

        // - Invoice Variables -------------

        vm.invoicesPerPage = 5;
        vm.invoices_next = [];
        vm.invoices_last = [];


        vm.showingInvoiceHistoryDateRange = false;
        vm.invoice_daterange_start = null;
        vm.invoice_daterange_end = null;



        /* =========================================================================

        BILLINGS PANEL SUBMENU

        ============================================================================*/


        vm.billingSubMenu =
            {
                dunningButton: false,
                activeButton: true,
                paymentHistoryButton: false,
                invoiceHistoryButton: false,

                update: function (buttonName) {

                    Debug.trace(buttonName + " clicked");

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
                    if (buttonName == 'invoiceHistory') {
                        vm.getInvoiceHistory();
                        this.dunningButton = false;
                        this.activeButton = false;
                        this.paymentHistoryButton = false;
                        this.invoiceHistoryButton = true;


                        //Initialize Date Range Picker(s)
                        $('.input-daterange').datepicker({
                            //todayHighlight: true
                        });

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

            Debug.trace("Getting payment transactions (initial list)... ");

            paymentServices.getPaymentHistory(vm.paymentsPerPage, vm.account.AccountID)
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
            Debug.trace("Preloading next payments... ");

            paymentServices.getPaymentHistory_Next(vm.paymentsPerPage, vm.payments[vm.payments.length - 1].ChargeID, vm.account.AccountID)
                        .success(function (data, status, headers, config) {

                            vm.paymentsPreloadingNext = false;
                            vm.payments_next = data;
                        })
                        .error(function (data, status, headers, config) {

                        })
            

        }

        vm.getPaymentHistory_Next = function () {

            Debug.trace("Getting next payments... ");

            vm.payments_last = vm.payments;
            vm.payments = vm.payments_next;
            vm.preload_NextPayments();

        }

        //---------------------------------------------------

        vm.preload_LastPayments = function () {
Transaction
            vm.paymentsPreloadingLast = true;

            vm.payments_last = [];

            Debug.trace("Preloading last payments... ");

            Debug.trace(vm.payments);

            paymentServices.getPaymentHistory_Last(vm.paymentsPerPage, vm.payments[0].ChargeID, vm.account.AccountID)
                        .success(function (data, status, headers, config) {

                            vm.paymentsPreloadingLast = false;
                            vm.payments_last = data;

                        })
                        .error(function (data, status, headers, config) {

                        })
            

        }

        vm.getPaymentHistory_Last = function () {

            Debug.trace("Getting last payments... ");

            vm.payments_next = vm.payments;
            vm.payments = vm.payments_last;
            vm.preload_LastPayments();
        }


        //--------------------------------------------------


        /* ==============================================

            PAYMENT/CHARGE DETAILS

            ============================================*/

        vm.loadingPaymentDetails = false;
        vm.showRefundsTab = true;

        vm.getPaymentDetail = function (index, reset)
        {
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
            /*
            
            vm.loadingPaymentDetails = true;

            invoiceServices.getInvoice(vm.paymentDetail.InvoiceID)
                        .success(function (data, status, headers, config) {


                            vm.paymentDetail.Invoice = data
                            Debug.trace(vm.paymentDetail.Invoice);
                            vm.loadingPaymentDetails = false;

                            //Refresh detail screen if index != null
                            if (vm.userDetail.Index != null) {
                                Debug.trace("Updating details for user index: " + vm.userDetail.Index);
                                vm.getUserDetail(vm.userDetail.Index, false);
                            }

                        })
                        .error(function (data, status, headers, config) {
                            //
                        })
           */
            
        }



        /* ==============================================

            PAYMENT/CHARGE REFUNDS

            ============================================*/

        vm.refundPayment =
            {

                ChargeID: null,
                RefundAmount: null,
                Index: null,

               update: {
                    edit: false,
                    processing: false,
                    complete: false,
                    isSuccess: false,
                    message: null
                },

                reset: function () {

                    this.ChargeID = null,
                    this.RefundAmount = null,
                    this.Index = null,

                    this.update.edit = false;
                    this.update.complete = false;
                    this.update.processing = false;

                    this.isSuccess = false;
                    this.message = null;

                }
            }


        vm.startPaymentRefund = function ()
        {
            vm.refundPayment.reset();
            vm.refundPayment.update.edit = true;

            vm.refundPayment.RefundAmount = (vm.paymentDetail.Amount - vm.paymentDetail.TotalRefunded).toFixed(2);
            vm.refundPayment.ChargeID = vm.paymentDetail.ChargeID;
            
            //Debug.trace(vm.refundPayment.RefundAmount + "|" + vm.paymentDetail.Amount);

        }
        vm.cancelRefundPayment = function()
        {
            vm.refundPayment.reset();
        }

        //vm.refreshRefundPaymentDetails = function () {
            //vm.refreshPaymentHistoryListAndDetails();
        //}
        vm.resetRefundPayment = function () {
            vm.refundPayment.update.edit = true;
            vm.refundPayment.update.processing = false;
            vm.refundPayment.update.complete = false;
            vm.refundPayment.update.isSuccess = false;
            vm.refundPayment.update.message = "";
        }
        vm.endRefundPayment = function () {

            //Hide refunds tab and open default
            vm.showRefundsTab = false;
            vm.paymentDetailsDefaultTabActive = true;

            vm.refundPayment.update.edit = false;
            vm.refundPayment.update.processing = false;
            vm.refundPayment.update.complete = false;
            vm.refundPayment.update.isSuccess = false;
            vm.refundPayment.update.message = "";
        }
        vm.applyRefund = function()
        {
            if (vm.isNumber(vm.refundPayment.RefundAmount) == false)
            {
                vm.refundPayment.update.edit = false;
                vm.refundPayment.update.complete = true;

                vm.refundPayment.update.isSuccess = false;
                vm.refundPayment.update.message = "Please use a monetary value with 2 decimal places. Examples: '2.25' or '.50'";
            }
            else
            {
                vm.refundPayment.update.edit = false;
                vm.refundPayment.update.processing = true;

                paymentServices.refundPayment(vm.account.AccountID, vm.refundPayment.ChargeID, vm.refundPayment.RefundAmount)
                .success(function (data, status, headers, config) {

                    vm.refundPayment.update.complete = true;
                    vm.refundPayment.update.processing = false;

                    if (data.isSuccess) {
                        vm.refundPayment.update.isSuccess = true;
                        vm.refundPayment.update.message = "A refund of $" + vm.refundPayment.RefundAmount + " has been applied to this charge!";

                        vm.refundPayment.update.edit = false;
                        vm.refundPayment.update.processing = false;
                        vm.refundPayment.update.complete = true;


                        vm.refreshPaymentHistoryListAndDetails();


                        //refresh payment details
                        //vm.getPaymentDetail(vm.refundPayment.Index, false)
                        // ...and List?
                        //vm.refreshPaymentHistory();

                    }
                    else {
                        vm.refundPayment.update.isSuccess = false;
                        vm.refundPayment.update.message = data.ErrorMessage;
                    }

                })
                            .error(function (data, status, headers, config) {
                                vm.refundPayment.update.complete = true;
                                vm.refundPayment.update.processing = false;
                                vm.refundPayment.update.isSuccess.isSuccess = false;
                                vm.refundPayment.update.message = "An error occurred contacting the service";
                            })
            }


        }

        vm.isNumber = function(n) {
            return !isNaN(parseFloat(n)) && isFinite(n);
        }


        //------------------------------------------------------------------

        //used to refresh latest list in the grid when making payment detail updates (refunds, etc...)
        vm.refreshPaymentHistoryListAndDetails = function () {
            //vm.paymentHistoryPanelLoading = true;



            //check if we are on the first page of the list, or deeper along:
            if (vm.payments_last.length > 0)
            {
                //if we are further along use the ChargeID of the last item in vm.payments_last

                paymentServices.getPaymentHistory_Next(vm.paymentsPerPage, vm.payments_last[vm.payments_last.length - 1].ChargeID, vm.account.AccountID)
                .success(function (data, status, headers, config) {

                    vm.payments = data;

                    paymentServices.getPayment(vm.refundPayment.ChargeID)
                    .success(function (data, status, headers, config) {

                        var invoice = vm.paymentDetail.Invoice; //<-- store invoice data
                        vm.paymentDetail = data;
                        vm.paymentDetail.Invoice = invoice; //<-- reapply to view model

                    })
                    .error(function (data, status, headers, config) {

                    })

                })
                .error(function (data, status, headers, config) {

                })

            }
            else
            {
                //Otherwise we call the initial histrical list
                paymentServices.getPaymentHistory(vm.paymentsPerPage, vm.account.AccountID)
                .success(function (data, status, headers, config) {

                    vm.payments = data;

                    paymentServices.getPayment(vm.refundPayment.ChargeID)
                    .success(function (data, status, headers, config) {

                        var invoice = vm.paymentDetail.Invoice; //<-- store invoice data
                        vm.paymentDetail = data;
                        vm.paymentDetail.Invoice = invoice; //<-- reapply to view model

                        //if (vm.payments.length == vm.paymentsPerPage) {
                            //vm.preload_NextPayments();
                        //}

                    })
                    .error(function (data, status, headers, config) {

                    })

                })
                .error(function (data, status, headers, config) {

                })

            }



        }




        /* ==========================================
            END PAYMENTS
        ==========================================*/







        /*========================================================

            INVOICES

        =========================================================*/


        //---------------------------------------------------

        vm.clearInvoiceDateRange = function ()
        {
            vm.invoice_daterange_start = null;
            vm.invoice_daterange_end = null;

            vm.showingInvoiceHistoryDateRange = false;

        }


        //---------------------------------------------------

        vm.invoiceNextPanelLoading = false;

        vm.getNextInvoice = function (accountId) {

            vm.invoiceNextPanelLoading = true;

            Debug.trace("Getting upcoming invoice... ");

            invoiceServices.getNextInvoice(accountId)
                        .success(function (data, status, headers, config) {

                            vm.invoiceNextPanelLoading = false
                            vm.account.NextInvoice = data;

                        })
                        .error(function (data, status, headers, config) {
                            //
                        })
        }

        //---------------------------------------------------

        vm.getInvoiceHistory = function () {

            vm.clearInvoiceDateRange();
            vm.invoices = [];
            vm.invoices_next = [];
            vm.invoices_last = [];

            vm.invoiceHistoryPanelLoading = true;

            Debug.trace("Getting invoice transactions (initial list)... ");

            invoiceServices.getInvoiceHistory(vm.invoicesPerPage, vm.account.AccountID)
                        .success(function (data, status, headers, config) {

                            vm.invoiceHistoryPanelLoading = false;
                            vm.invoices = data;

                            //vm.InvoiceIDIndex1 = vm.invoices[0].InvoiceID

                            if(vm.invoices.length == vm.invoicesPerPage)
                            {
                                vm.preload_NextInvoices();
                            }

                        })
                        .error(function (data, status, headers, config) {
                            vm.invoiceHistoryPanelLoading = false;
                        })
        }

        // ---------------------------------------------------

        vm.filterInvoicesByDateRange = function () {

            vm.invoices = [];
            vm.invoices_next = [];
            vm.invoices_last = [];

            vm.invoiceHistoryPanelLoading = true;

            Debug.trace("Getting invoice transactions (by date range)... ");

            invoiceServices.getInvoiceHistory_ByDateRange(vm.invoicesPerPage, vm.invoice_daterange_start, vm.invoice_daterange_end, vm.account.AccountID)
                        .success(function (data, status, headers, config) {

                            vm.showingInvoiceHistoryDateRange = true;

                            vm.invoiceHistoryPanelLoading = false;
                            vm.invoices = data;

                            //vm.InvoiceIDIndex1 = vm.invoices[0].InvoiceID

                            if (vm.invoices.length == vm.invoicesPerPage) {
                                vm.preload_NextInvoices();
                            }

                        })
                        .error(function (data, status, headers, config) {
                            vm.invoiceHistoryPanelLoading = false;
                        })

        }

        //---------------------------------------------------

        vm.preload_NextInvoices = function()
        {
            vm.invoicesPreloadingNext = true;

            vm.invoices_next = [];
            Debug.trace("Preloading next invoices... ");


            if(vm.invoice_daterange_start == null && vm.invoice_daterange_end == null)
            {
                invoiceServices.getInvoiceHistory_Next(vm.invoicesPerPage, vm.invoices[vm.invoices.length - 1].InvoiceID, vm.account.AccountID)
                            .success(function (data, status, headers, config) {

                                vm.invoicesPreloadingNext = false;
                                vm.invoices_next = data;
                            })
                            .error(function (data, status, headers, config) {

                            })
            }
            else
            {
                invoiceServices.getInvoiceHistory_ByDateRange_Next(vm.invoicesPerPage, vm.invoice_daterange_start, vm.invoice_daterange_end, vm.invoices[vm.invoices.length - 1].InvoiceID, vm.account.AccountID)
                            .success(function (data, status, headers, config) {

                                vm.invoicesPreloadingNext = false;
                                vm.invoices_next = data;
                            })
                            .error(function (data, status, headers, config) {

                            })
            }

        }

        vm.getInvoiceHistory_Next = function () {

            Debug.trace("Getting next invoices... ");

            vm.invoices_last = vm.invoices
            vm.invoices = vm.invoices_next
            vm.preload_NextInvoices();

        }

        //---------------------------------------------------

        vm.preload_LastInvoices = function () {

            vm.invoicesPreloadingLast = true;

            vm.invoices_last = [];

            Debug.trace("Preloading last invoices... ");

            Debug.trace(vm.invoices);

            if (vm.invoice_daterange_start == null && vm.invoice_daterange_end == null) {

                invoiceServices.getInvoiceHistory_Last(vm.invoicesPerPage, vm.invoices[0].InvoiceID, vm.account.AccountID)
                            .success(function (data, status, headers, config) {

                                vm.invoicesPreloadingLast = false;
                                vm.invoices_last = data;

                            })
                            .error(function (data, status, headers, config) {
                            
                            })
            }
            else
            {
                invoiceServices.getInvoiceHistory_ByDateRange_Last(vm.invoicesPerPage, vm.invoice_daterange_start, vm.invoice_daterange_end, vm.invoices[0].InvoiceID, vm.account.AccountID)
                            .success(function (data, status, headers, config) {

                                vm.invoicesPreloadingLast = false;
                                vm.invoices_last = data;

                            })
                            .error(function (data, status, headers, config) {

                            })
            }

        }


        vm.getInvoiceHistory_Last = function () {

            Debug.trace("Getting last invoices... ");

            vm.invoices_next = vm.invoices;
            vm.invoices = vm.invoices_last;
            vm.preload_LastInvoices();
        }


        /* ==========================================
            END INVOICES
        ==========================================*/
        /* ==============================================

        INVOICE DETAILS

        ============================================*/

        vm.loadingInvoiceDetails = false;

        vm.getInvoiceDetail = function (index, reset) {


            vm.invoiceDetail = vm.invoices[index];
            vm.invoiceDetail.Index = index;

        }


        /* =========================================

            DUNNING ATTEMPTS

        ==========================================*/




        vm.getDunningAttempts = function () {

            vm.dunningAttempts = [];

            vm.dunningAttemptsPanelLoading = true;

            Debug.trace("Getting dunning attempts... ");

            paymentServices.getDunningAttempts(vm.account.AccountID)
                        .success(function (data, status, headers, config) {

                            vm.dunningAttemptsPanelLoading = false;
                            vm.dunningAttempts = data;


                        })
                        .error(function (data, status, headers, config) {
                            vm.dunningAttemptsPanelLoading = false;
                        })
        }




        /* ==========================================
            END DUNNING ATTEMPTS
        ==========================================*/

        /* ==========================================
            END BILLING PANEL
        ==========================================*/

















        /* =========================================================================

            COMMERCE TAB

         ============================================================================*/

        /*============================================================
            COMMERCE VARIABLES
        =======================================================*/

        vm.creditsToDollarExchangeRate = null;
        vm.availableCredits = 0;
        vm.creditsTransactionLog = null;

        /*============================================================
            COMMERCE SUBMENU
        =======================================================*/

        vm.commerceSubMenu =
            {
                creditsButton: true,
                ordersButton: false,


                update: function (buttonName) {

                    Debug.trace(buttonName + " clicked");

                    if (buttonName == 'credits') {
                        //vm.getCredits(vm.account.AccountID);
                        this.creditsButton = true;
                        this.ordersButton = false;

                    }
                    if (buttonName == 'orders') {
                        //vm.activateInvitationsPanel();
                        this.creditsButton = false;
                        this.ordersButton = true;

                    }
                },
            }

        /*============================================================
            COMMERCE METHODS
        =======================================================*/

        vm.activateCommercePanel = function()
        {
            //get exchange rate
            vm.getExchangeRate();

            //load credits for the account
            vm.getAvailableCredits();

            //Load transaction log
            vm.getCreditsTransactionLog();
        }


        vm.getExchangeRate = function () {

            Debug.trace("Getting exchange rate...");

            commerceServices.getCreditsToDollarExchangeValue()
            .success(function (data, status, headers, config) {


                vm.creditsToDollarExchangeRate = eval(data);

                Debug.trace("Data:" + vm.creditsToDollarExchangeRate + data);

            })
            .error(function (data, status, headers, config) {

            })
        }

        vm.getAvailableCredits = function () {

            Debug.trace("Getting available credits...");

            commerceServices.getAvailableCredits(vm.account.AccountID)
            .success(function (data, status, headers, config) {

                var countUpOptions = {
                    useEasing: true,
                    useGrouping: true,
                    separator: ',',
                    decimal: '.',
                    prefix: '',
                    suffix: '',
                }

                var previousCredits = 0;

                try
                {
                    previousCredits = vm.availableCredits;
                }
                catch (err)
                {
                }

                try
                {
                    var countUpExecution = new countUp("countUpElement", previousCredits, eval(data), 0, 1.5, countUpOptions);              
                    countUpExecution.start();
                    vm.availableCredits = eval(data);
                }
                catch(err)
                {
                    vm.availableCredits = eval(data);
                }

            })
            .error(function (data, status, headers, config) {

            })
        }

        // Credits Transaction Log -------------------------

        vm.getCreditsTransactionLog = function () {
            Debug.trace("Getting credits transaction log...");

            setTimeout(function () {


                accountDetailServices.getAccountLogByCategory(vm.account.AccountID, "Credits", 15)
                .success(function (data, status, headers, config) {

                    vm.creditsTransactionLog = data;
                })
                .error(function (data, status, headers, config) {

                })


            }, 1000);


        }




















        /* ==========================================
            
            LOGS

        ==========================================*/

        vm.accountLog = null;

        vm.accountLogCategory = null;
        vm.accountLogActivity = null;
        vm.accountLogUserId = null;
        vm.accountLogObjectId = null;


        vm.accountLogRecordResultCounts = [
            { label: '50 records',      value: 50 },
            { label: '100 records',     value: 100 },
            { label: '250 records',     value: 250 },
            { label: '500 records',     value: 500 },
            { label: '750 records',     value: 750 },
            { label: '1,000 records',   value: 1000 },
        ]

        vm.accountLogCategories = [];
        vm.accountLogActivities = [];


        vm.currentAccountLogRecordResultCount = vm.accountLogRecordResultCounts[0];
        vm.updateAccountLogRecordResultCount = function(count)
        {
            vm.currentAccountLogRecordResultCount = count;
        }


        /*============================================================
                    LOGS SUBMENU
        =======================================================*/

        vm.logsSubMenu =
            {
                allButton: true,
                categoryButton: false,
                activityButton: false,
                userButton: false,

                update: function (buttonName) {

                    Debug.trace(buttonName + " clicked");

                    if (buttonName == 'all') {
                        this.allButton = true,
                        this.categoryButton = false,
                        this.activityButton = false,
                        this.userButton = false
                    }
                    if (buttonName == 'category') {
                        this.allButton = false,
                        this.categoryButton = true,
                        this.activityButton = false,
                        this.userButton = false
                    }
                    if (buttonName == 'activity') {
                        this.allButton = false,
                        this.categoryButton = false,
                        this.activityButton = true,
                        this.userButton = false
                    }
                    if (buttonName == 'user') {
                        this.allButton = false,
                        this.categoryButton = false,
                        this.activityButton = false,
                        this.userButton = true
                    }
                },
            }

        vm.accountLog = null;

        vm.accountLogCategory = null;
        vm.accountLogActivity = null;
        vm.accountLogUser = null;
        //vm.accountLogObjectId = null;

        // Adjust filters ------------

        vm.updateCurrentLogFilterCategory = function (category) {
            vm.accountLogCategory = category
        }

        vm.updateCurrentLogFilterActivity = function (activity) {
            vm.accountLogActivity = activity
        }

        vm.updateCurrentLogFilterUser = function(user)
        {
            vm.accountLogUser = user
        }


        /*============================================================
            LOG METHODS
        =======================================================*/

        vm.activateLogPanel = function()
        {
            // Load initial log set
            vm.getAccountLog(vm.currentAccountLogRecordResultCount.value);
            
            // Load Category List
            accountDetailServices.getAccountLogCategories()
            .success(function (data, status, headers, config) {
                vm.accountLogCategories = data;
                vm.accountLogCategory = data[0];
            })
            .error(function (data, status, headers, config) {

            })


            // Load Activity List
            accountDetailServices.getAccountLogActivities()
            .success(function (data, status, headers, config) {
                vm.accountLogActivities = data;
                vm.accountLogActivity = data[0];
            })
            .error(function (data, status, headers, config) {

            })


            // Load user list
            vm.accountLogUser = vm.account.Users[0];

        }

        // Update Logs ----------------------------

        vm.updateAccountLog = function()
        {
            if(vm.logsSubMenu.allButton)
            {
                vm.getAccountLog(vm.currentAccountLogRecordResultCount.value);
            }
            else if(vm.logsSubMenu.categoryButton)
            {
                vm.getAccountLogByCategory(vm.accountLogCategory, vm.currentAccountLogRecordResultCount.value);
            }
            else if (vm.logsSubMenu.activityButton)
            {
                vm.getAccountLogByActivity(vm.accountLogActivity, vm.currentAccountLogRecordResultCount.value);
            }
            else if (vm.logsSubMenu.userButton)
            {
                vm.getAccountLogByUser(vm.accountLogUser.Id, vm.currentAccountLogRecordResultCount.value);
            }
            
        }

        

        // Basic Log Calls ------------------------

        vm.getAccountLog = function (count) {

            vm.accountLogLoading = true;

            accountDetailServices.getAccountLog(vm.account.AccountID, count)
            .success(function (data, status, headers, config) {
                vm.accountLog = data;
                vm.accountLogLoading = false;
            })
            .error(function (data, status, headers, config) {
                vm.accountLogLoading = false;
            })
        }

        vm.getAccountLogByCategory = function (category, count) {

            vm.accountLogLoading = true;

            accountDetailServices.getAccountLogByCategory(vm.account.AccountID, category, count)
            .success(function (data, status, headers, config) {

                vm.accountLog = data;
                vm.accountLogLoading = false;
            })
            .error(function (data, status, headers, config) {
                vm.accountLogLoading = false;
            })
        }

        vm.getAccountLogByActivity = function (activity, count) {

            vm.accountLogLoading = true;

            accountDetailServices.getAccountLogByActivity(vm.account.AccountID, activity, count)
            .success(function (data, status, headers, config) {

                vm.accountLog = data;
                vm.accountLogLoading = false;
            })
            .error(function (data, status, headers, config) {
                vm.accountLogLoading = false;
            })
        }

        vm.getAccountLogByUser = function (userId, count) {

            vm.accountLogLoading = true;

            accountDetailServices.getAccountLogByUser(vm.account.AccountID, userId, count)
            .success(function (data, status, headers, config) {

                vm.accountLog = data;
                vm.accountLogLoading = false;
            })
            .error(function (data, status, headers, config) {
                vm.accountLogLoading = false;
            })
        }

        vm.getAccountLogByObject = function (objectId, count) {

            vm.accountLogLoading = true;

            accountDetailServices.getAccountLogByObject(vm.account.AccountID, objectId, count)
            .success(function (data, status, headers, config) {

                vm.accountLog = data;
                vm.accountLogLoading = false;
            })
            .error(function (data, status, headers, config) {
                vm.accountLogLoading = false;
            })
        }


        vm.getLogDetail = function (index, reset) {

            vm.logDetail = vm.accountLog[index];
            //vm.paymentDetail.Index = index;

        }













        /* ==========================================
            
            START COMMUNICATIONS PANEL

        ==========================================*/


        vm.newNotification = {

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

            reset: function (type) {

                this.message = "";
                this.expirationMinutes = 0;
                this.setExpiration = false;

                this.results.isSuccess = null;
                this.results.message = null;

                this.update.processing = false;
                this.update.complete = false;

                vm.currentNotificationType = vm.notificationTypes[0];
                vm.currentNotificationRecipient = vm.notificationRecipients[0];
                vm.currentNotificationUser = vm.account.Users[0];
            },

            retry: function () {

                this.update.processing = false;
                this.update.complete = false;
            }
        }

        vm.activateSendNotificationModal = function (notificationType) {
            vm.newNotification.reset(notificationType);
        }

        vm.sendNotification = function () {

            vm.newNotification.update.processing = true;

            if (vm.newNotification.setExpiration == false) {
                vm.newNotification.expirationMinutes = 0
            }

            if (vm.currentNotificationRecipient.value == 'one')
            {

                sharedServices.sendNotificationToUser(
                    vm.currentNotificationType.value,
                    vm.currentNotificationUser.Id,
                    vm.newNotification.message,
                    vm.newNotification.expirationMinutes
                ).success(function (data, status, headers, config) {

                    vm.newNotification.update.processing = false;
                    vm.newNotification.update.complete = true;

                    if (data.isSuccess) {

                        vm.newNotification.results.isSuccess = true;

                        vm.newNotification.results.message = "'" + vm.currentNotificationType.value + "' notification has been sent to " + vm.currentNotificationUser.FirstName + " " + vm.currentNotificationUser.LastName;

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
            else
            {
                var ownersOnly = true;
                if (vm.currentNotificationRecipient.value == 'all')
                {
                    ownersOnly = false;
                }

                sharedServices.sendNotificationToAccount(
                    vm.currentNotificationType.value,
                    vm.account.AccountID,
                    vm.newNotification.message,
                    vm.newNotification.expirationMinutes,
                    ownersOnly
                ).success(function (data, status, headers, config) {

                    vm.newNotification.update.processing = false;
                    vm.newNotification.update.complete = true;

                    if (data.isSuccess) {

                        vm.newNotification.results.isSuccess = true;
                        if (ownersOnly)
                        {
                            vm.newNotification.results.message = "'" + vm.currentNotificationType.value + "' notification has been sent to all account owners.";

                        }
                        else
                        {
                            vm.newNotification.results.message = "'" + vm.currentNotificationType.value + "' notification has been sent to all account users.";

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


        }




        //---Notification Recipients --------------

        vm.currentNotificationRecipient = null

        vm.updateCurrentNotificationRecipient = function (recipient) {
            vm.currentNotificationRecipient = recipient;

            Debug.trace('Updated notifications recipient to: ' + vm.currentNotificationRecipient.label);

        }

        //---Notification User --------------

        vm.currentNotificationUser = null

        vm.updateCurrentNotificationUser = function (user) {
            vm.currentNotificationUser = user;

            Debug.trace('Updated notification user to: ' + vm.currentNotificationUser.FirstName + " " + vm.currentNotificationUser.LastName);

        }

        //---Notification Types --------------

        vm.currentNotificationType = null

        vm.updateCurrentNotificationType = function (type) {
            vm.currentNotificationType = type;

            Debug.trace('Updated notification type to: ' + vm.currentNotificationType.label);

        }


        // Notification Helpers ----------------

        vm.generateExpirationDate = function (expirationMinutes) {
            var d = new Date();
            d.setMinutes(d.getMinutes() + +expirationMinutes);
            return d;
        }














        /* ==========================================

           DOCUMENT PARTITION PANEL

       ==========================================*/



        vm.activateDocumentPartitionPanel = function()
        {
            vm.documentCollectionProperties = null;
            platformServices.getDocumentPartitionTenantCollectionProperties(vm.account.AccountID)
            .success(function (data, status, headers, config) {

                vm.documentCollectionProperties = data;
                Debug.trace("Tenant partition collection properties returned.");

            })
            .error(function (data, status, headers, config) {
                //
            })
        }


        /* ==========================================

           END DOCUMENT PARTITION PANEL

       ==========================================*/













        /* ==========================================

           SQL PARTITION PANEL

        ==========================================*/



        vm.activateSqlPartitionPanel = function () {
            vm.sqlTenantLog = null;
            platformServices.getSqlPartitionTenantSchemaLog(vm.account.AccountID)
            .success(function (data, status, headers, config) {

                vm.sqlTenantLog = data;
                Debug.trace("Tenant partition sql log returned.");

            })
            .error(function (data, status, headers, config) {
                //
            })
        }


        /* ==========================================

           END SQL PARTITION PANEL

       ==========================================*/




        /* ==========================================

           ACCOUNT UNPROVISIONED CLOSURE PANEL

        ==========================================*/


        vm.accountUnprovisionedClosure = {
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

        vm.startAccountUnprovisionedClosure = function () {
            vm.accountUnprovisionedClosure.Clear();
            vm.accountUnprovisionedClosure.warningState = true;
        }

        vm.cancelAccountUnprovisionedClosure = function () {
            vm.accountUnprovisionedClosure.Clear();
            vm.accountUnprovisionedClosure.defaultState = true;
        }



        vm.processAccountUnprovisionedClosure = function () {

            vm.accountUnprovisionedClosure.Clear();
            vm.accountUnprovisionedClosure.processingState = true;

            accountDetailServices.closeUnprovisionedAccount(vm.account.AccountID)
                        .success(function (data, status, headers, config) {

                            vm.accountUnprovisionedClosure.Clear();
                            vm.accountUnprovisionedClosure.completedState = true;

                            if (data.isSuccess) {
                                vm.accountUnprovisionedClosure.isSuccess = true;
                                vm.accountUnprovisionedClosure.message = data.SuccessMessage;
                            }
                            else {
                                vm.accountUnprovisionedClosure.isSuccess = false;
                                vm.accountUnprovisionedClosure.message = data.ErrorMessage;
                            }


                        })
                        .error(function (data, status, headers, config) {

                            vm.accountUnprovisionedClosure.Clear();
                            vm.accountUnprovisionedClosure.completedState = true;
                            vm.accountUnprovisionedClosure.isSuccess = false;
                            vm.accountUnprovisionedClosure.message = "An error occured while attempting to use the service.";

                        })


        }



        vm.backToAccounts = function () {

            window.location.href = "/accounts";
        }






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

        vm.startAccountClosure = function () {
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

            accountDetailServices.closeAccount(vm.account.AccountID)
                        .success(function (data, status, headers, config) {

                            vm.accountClosure.Clear();
                            vm.accountClosure.completedState = true;

                            if (data.isSuccess) {
                                vm.accountClosure.isSuccess = true;
                                vm.accountClosure.message = data.SuccessMessage;
                            }
                            else {
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


        /* ----------- Closure Status ----------------*/

        vm.closureApprovalRequired = null;
        vm.reactivateSubscription = false;
        vm.reactivatingSubscription = false;

        vm.getClosureStatus = function()
        {
            accountDetailServices.doesAccountRequireClosureApproval(vm.account.AccountID)
                        .success(function (data, status, headers, config) {
                            vm.closureApprovalRequired = JSON.parse(data);
                        })
                        .error(function (data, status, headers, config) {
                        })
        }

        /* -------------- Closure Approval & Approval Reversal---------------- */

        vm.approveClosure = function () {

            vm.closureApprovalRequired = null;

            accountDetailServices.approveAccountClosure(vm.account.AccountID)
                        .success(function (data, status, headers, config) {
                            if (data.isSuccess)
                            {
                                vm.closureApprovalRequired = false;
                            }
                            else
                            {
                                vm.closureApprovalRequired = true;
                            }
                        })
                        .error(function (data, status, headers, config) {
                        })
        }


        vm.reverseClosureApproval = function () {

            vm.closureApprovalRequired = null;

            accountDetailServices.reversAccounteClosureApproval(vm.account.AccountID)
                        .success(function (data, status, headers, config) {
                            if (data.isSuccess) {
                                vm.closureApprovalRequired = true;
                            }
                            else {
                                vm.closureApprovalRequired = false;
                            }
                        })
                        .error(function (data, status, headers, config) {
                        })
        }


        /* ----------- Subscription Reactivation ----------------*/

        vm.reactivateSubscription = function () {

            vm.reactivatingSubscription = true;

            vm.closureApprovalRequired = null;
            vm.reactivateSubscription = null;

            accountDetailServices.reactivateSubscription(vm.account.AccountID)
                        .success(function (data, status, headers, config) {
                            vm.reactivateSubscription = data;
                        })
                        .error(function (data, status, headers, config) {

                        })
        }

        vm.refreshAccount = function(tabSetting)
        {
            vm.refresh();
            vm.closureTabActive = tabSetting;
            //vm.getAccount();
        }


        /* ==========================================

           END ACCOUNT CLOSURE PANEL

       ==========================================*/





        /* ==========================================
        
            ACCELERATE ACCOUNT CLOSURE 
        
        ==========================================*/

        vm.accelerateAccountClosureAccept = false;
        vm.accelerateAccountClosureComplete = false;
        vm.accelerateAccountClosureProcessing = false;

        vm.startAccelerateAccountClosure = function()
        {
            vm.accelerateAccountClosureAccept = true;
        }
        vm.cancelAccelerateAccountClosure = function () {
            vm.accelerateAccountClosureAccept = false;
        }
        vm.accelerateAccountClosure = function () {
            vm.accelerateAccountClosureProcessing = true;
            accountDetailServices.accelerateAccountClosure(vm.account.AccountID)
                        .success(function (data, status, headers, config) {
                            vm.accelerateAccountClosureProcessing = false;
                            if (data.isSuccess) {
                                vm.accelerateAccountClosureComplete = true;
                            }
                            else {
                                
                            }

                        })
                        .error(function (data, status, headers, config) {
                            vm.accelerateAccountClosureProcessing = false;
                        })

        }








        /* ==========================================

           DATA INJECTION PANEL

       ==========================================*/

        vm.imageDocumentDataInjectionResult = false;
        vm.imageDocumentDataInjectionCount = 0;

        vm.injectImageDocuments = function () {

            vm.imageDocumentDataInjectionResult = null;
            accountDetailServices.injectImageDocuments(vm.account.AccountID, vm.imageDocumentDataInjectionCount)
                        .success(function (data, status, headers, config) {

                            vm.imageDocumentDataInjectionResult = data;
                            if (data.isSuccess)
                            {
                                vm.imageDocumentDataInjectionCount = 0;
                            }
                            
                        })
                        .error(function (data, status, headers, config) {
                            vm.imageDocumentDataInjectionCount = 0;
                        })
        }

        vm.clearDocumentInjectionPanel = function () {
            vm.imageDocumentDataInjectionResult = false;
            vm.imageDocumentDataInjectionCount = 0;
        }

        /* ==========================================

           END DATA INJECTION PANEL

       ==========================================*/












        /* ==========================================
           CURRENT USER PROFILE
       ==========================================*/

        function updateCurrentUserProfile() {
            
            Debug.trace("Refreshing user profile...");

            sharedServices.getCurrentUserProfile()
            .success(function (data, status, headers, config) {

                vm.currentUserProfile = data; //Used to determine what is shown in the view based on user Role.
                currentUserRoleIndex = vm.platformRoles.indexOf(data.Role) //<-- use PLATFORM roles, NOT ACCOUNT roles! <-- Role will indicate what editing capabilites are available.

                Debug.trace("Profile refreshed!");
                Debug.trace("Role index = " + currentUserRoleIndex);

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

        function activate() {

            vm.showGetAccountLoading = true;

            // Injected variables from the view (via CoreServices/PlatformSettings)
            //Platform --------------------------------------------
            vm.CustodianFrequencyDescription = CoreServiceSettings_Custodian_FrequencyDescription;
            vm.PlatformWorkerFrequencyDescription = CoreServiceSettings_PlatformWorker_FrequencyDescription;

            vm.AccountManagementDomain = CoreServiceSettings_AccountManagementDomain;
            vm.AccountApiDomain = CoreServiceSettings_AccountApiDomain;
            vm.AccountSiteDomain = CoreServiceSettings_AccountSiteDomain;

            //CDN URL for User Images
            //vm.cdnUri = JSON.parse(CoreServiceSettings_Urls_AccountImagesCdnUri);
            //Accounts ------------------------------------------
            vm.userRoles = JSON.parse(CoreServiceSettings_AccountUsers_RolesList);
            //vm.selectedRole = vm.userRoles[0];
            vm.newUser.Role = vm.userRoles[0];

            //Platform Roles (used for the logged in Platform user, to check Roles accesability
            vm.platformRoles = JSON.parse(CoreServiceSettings_PlatformUsers_RolesList);

            //URLs
            vm.accountUserInvitationUrl = CoreServiceSettings_AccountUsers_InvitationUrl;
            vm.accountUserPasswordClaimUrl = CoreServiceSettings_AccountUsers_PasswordClaimUrl;

            vm.notificationRecipients = accountDetailModels.getNotificationRecipientTypes();
            vm.currentNotificationRecipient = vm.notificationRecipients[0];

            vm.notificationTypes = accountDetailModels.getNotificationTypes();
            vm.currentNotificationType = vm.notificationTypes[0];


            vm.getAccount();

            // Load local profile for the platfor user.
            vm.currentUserProfile = JSON.parse(CurrentUserProfile);
            currentUserRoleIndex = vm.platformRoles.indexOf(vm.currentUserProfile.Role) //<-- Role will indicate what editing capabilites are available.
            // Refresh the profile every 45 seconds (if Role is updated, new editing capabilites will light up for the user)
            setInterval(function () { updateCurrentUserProfile() }, 150000);

            Debug.trace('AccountDetail Controller activation complete');



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
