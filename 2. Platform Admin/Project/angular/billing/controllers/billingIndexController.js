(function () {
    'use strict';

    var controllerId = 'billingIndexController';

    // Inject into main app module:
    angular.module('app')
        .controller(controllerId, [
            'billingServices',
            'billingIndexModels',
            'sharedServices',
            'paymentServices',
            'invoiceServices',
            'transferServices',
            'plansServices',
            'platformServices',
             billingIndexController
    ]);

    function billingIndexController(billingServices, billingIndexModels, sharedServices, paymentServices, invoiceServices, transferServices, plansServices, platformServices) {

        //Instantiate Controller as ViewModel
        var vm = this;

        //Default Properties: =============================
        vm.title = 'billingIndexController';
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

        // vm.TrialHoldDays = null; //<----comes from CoreServices (via local feed)
        //vm.CustodianFrequencyDescription = null; //<----comes from CoreServices (via local feed)
        //vm.UnverifiedAccountsDaysToHold = null; //<----comes from CoreServices (via local feed)
        //vm.PlatformWorkerFrequencyDescription = null; //<----comes from CoreServices (via local feed)



        /* ==========================================
                Controller Properties
        ==========================================*/



        /* ==========================================

            SNAPSHOT

        ==========================================*/

        vm.snapshot = null;


        vm.getSnapshot = function () {

            Debug.trace('Getting snapshot...');

            billingServices.getSnapshot()
                .success(function (data, status, headers, config) {
                    vm.snapshot = data;
                })
                .error(function (data, status, headers, config) {

                });
        }


        /* ==========================================

            END SNAPSHOT

        ==========================================*/




        /* ==========================================

            REPORT

        ==========================================*/

        vm.report = null;


        vm.reportLengths = [
                    { label: 'Last 24 Hours',   labelLower: 'last 24 Hours',    hours: 24 },
                    { label: 'Last 48 Hours',   labelLower: 'last 48 Hours',    hours: 48 },
                    { label: 'Last 3 Days',     labelLower: 'last 3 Days',      hours: 72 },
                    { label: 'Last 7 Days',     labelLower: 'last 7 Days',      hours: 168 },
                    { label: 'Last 14 Days',    labelLower: 'last 14 Days',     hours: 336 },
                    { label: 'Last 30 Days',    labelLower: 'last 30 Days',     hours: 720 },
                    { label: 'Last 60 Days',    labelLower: 'last 60 Days',     hours: 1440 },
                    { label: 'Last 90 Days',    labelLower: 'last 90 Days',     hours: 2160 },
                    { label: 'Last 120 Days',   labelLower: 'last 120 Days',    hours: 2880 },
        ];
        

        vm.currentReportLength = vm.reportLengths[0];
        vm.currentReportLengthForDisplay = vm.reportLengths[0];

        vm.setCurrentReportLength = function (reportLength)
        {
            vm.currentReportLength = reportLength;
        }


        vm.getReport = function () {

            vm.report = null;
            Debug.trace('Getting report...');

            billingServices.getReport(vm.currentReportLength.hours)
                .success(function (data, status, headers, config) {
                    vm.report = data;

                    vm.currentReportLengthForDisplay = vm.currentReportLength;
                })
                .error(function (data, status, headers, config) {

                });
        }


        /* ==========================================

            END REPORT

        ==========================================*/







        /* ============================================================

           
           Payment Plan Panel Directive


       ================================================================*/




        //Models, etc...
        vm.paymentPlans = null;
        vm.paymentFrequencies = null;


        vm.activatePaymentPlansPanel = function ()
        {
            vm.refreshPaymentPlans();
        }


        vm.refreshPaymentPlans = function () {
            //Debug.trace("getting plans...");
            plansServices.getPlans()
                .success(function (data, status, headers, config) {
                    //Debug.trace(data);
                    vm.paymentPlans = data;

                    //Debug.trace(vm.paymentPlans);

                    plansServices.getFrequencies()
                            .success(function (data, status, headers, config) {
                                //Debug.trace(data);
                                vm.paymentFrequencies = data;

                            })
                            .error(function (data, status, headers, config) {

                            })

                })
                .error(function (data, status, headers, config) {

                })
        }


        /* =========================================================================================
            Payment Plan Details
        =============================================================================================*/

        vm.planDetailsPanelLoading = true;

        vm.getPlanDetail = function (index, reset) {

            if (reset == true) {
                Debug.trace("Resetting details...");
                vm.planDetail.reset();
                vm.planDeletion.reset();
            }

            vm.planDetail.Index = index;

            //Assign selected row to invitationDetail object ------------------------------------------------
            //vm.userDetail.InvitationKey = vm.invitations[index].InvitationKey;
            
            vm.planDetail.Name = vm.paymentPlans[index].PaymentPlanName;
            vm.planDetail.Visible = vm.paymentPlans[index].Visible;
            vm.planDetail.MonthlyRate = vm.paymentPlans[index].MonthlyRate;

            vm.planDetail.MaxUsers = vm.paymentPlans[index].MaxUsers;

            vm.planDetail.MaxCategorizationsPerSet = vm.paymentPlans[index].MaxCategorizationsPerSet;
            vm.planDetail.MaxCategorizations = vm.paymentPlans[index].MaxCategorizations;
            vm.planDetail.MaxProductsPerSet = vm.paymentPlans[index].MaxProductsPerSet;
            vm.planDetail.MaxProducts = vm.paymentPlans[index].MaxProducts;


            vm.planDetail.MaxValuesPerProperty = vm.paymentPlans[index].MaxValuesPerProperty;
            vm.planDetail.MaxImageGroups = vm.paymentPlans[index].MaxImageGroups;
            vm.planDetail.MaxImageFormats = vm.paymentPlans[index].MaxImageFormats;
            vm.planDetail.MaxImageGalleries = vm.paymentPlans[index].MaxImageGalleries;
            vm.planDetail.MaxImagesPerGallery = vm.paymentPlans[index].MaxImagesPerGallery;


            vm.planDetail.MaxProperties = vm.paymentPlans[index].MaxProperties;
            vm.planDetail.MaxTags = vm.paymentPlans[index].MaxTags;

            vm.planDetail.BasicSupport = vm.paymentPlans[index].BasicSupport;
            vm.planDetail.EnhancedSupport = vm.paymentPlans[index].EnhancedSupport;

            vm.planDetail.AllowCustomOrdering = vm.paymentPlans[index].AllowCustomOrdering;
            vm.planDetail.AllowSalesLeads = vm.paymentPlans[index].AllowSalesLeads;
            vm.planDetail.AllowLocationData = vm.paymentPlans[index].AllowLocationData;
            vm.planDetail.AllowThemes = vm.paymentPlans[index].AllowThemes;
            vm.planDetail.AllowImageEnhancements = vm.paymentPlans[index].AllowImageEnhancements;
            vm.planDetail.MonthlySupportHours = vm.paymentPlans[index].MonthlySupportHours;

            vm.planDetail.Variants = [];

            if (vm.planDetail.MonthlyRate)
            {
                for (var i = 0; i < vm.paymentFrequencies.length; ++i) {
                    if (vm.paymentFrequencies[i].PaymentFrequencyMonths > 0)
                    {
                        Debug.trace(vm.planDetail.Name + "-" + vm.paymentFrequencies[i].IntervalCount + "-" + vm.paymentFrequencies[i].Interval);
                        vm.planDetail.Variants.push(vm.planDetail.Name + "-" + vm.paymentFrequencies[i].IntervalCount + "-" + vm.paymentFrequencies[i].Interval)
                    }
                    
                    
                }
            }
        }

        /* ==========================================
          BASIC TAB
       ==========================================*/


        //Unbindable properties for editing basic plan state
        var editingPlan =
            {
                //newName: null,
                newMaxUsers: null,
                newMaxCategories: null,
                newMaxSubcategories: null,
                newMaxTags: null,
            }

        vm.planDetail =
            {
                DefaultTabActive: true,

                Index: null,

                Name: null,
                Visible: false,
                MonthlyRate: 0.0,

                MaxUsers: 0,
                MaxCategories: 0,
                MaxSubcategories: 0,
                MaxTags: 0,
                
                Variants: [],

                updateName: {
                    edit: false,
                    processing: false,
                    complete: false,
                    isSuccess: false,
                    message: null
                },

                updateMaxUsers: {
                    edit: false,
                    processing: false,
                    complete: false,
                    isSuccess: false,
                    message: null
                },
                updateMaxCategories: {
                    edit: false,
                    processing: false,
                    complete: false,
                    isSuccess: false,
                    message: null
                },
                updateMaxSubcategories: {
                    edit: false,
                    processing: false,
                    complete: false,
                    isSuccess: false,
                    message: null
                },
                updateMaxTags: {
                    edit: false,
                    processing: false,
                    complete: false,
                    isSuccess: false,
                    message: null
                },
                updateMaxImages: {
                    edit: false,
                    processing: false,
                    complete: false,
                    isSuccess: false,
                    message: null
                },
                updateAllowImageEnhancements: {
                    processing: false,
                    complete: false,
                    isSuccess: false,
                    message: null,
                },
                updateVisibleState: {
                    processing: false,
                    complete: false,
                    isSuccess: false,
                    message: null,
                },

                reset: function () {

                    this.DefaultTabActive = true;
                    this.Variants = [];

                    //this.updateName.edit = false;
                    this.updateMaxUsers.edit = false;
                    this.updateMaxCategories.edit = false;
                    this.updateMaxSubcategories.edit = false;
                    this.updateMaxTags.edit = false;
                    this.updateMaxImages.edit = false;

                    //this.updateName.complete = false;
                    this.updateMaxUsers.complete = false;
                    this.updateMaxCategories.complete = false;
                    this.updateMaxSubcategories.complete = false;
                    this.updateMaxTags.complete = false;
                    this.updateMaxImages.complete = false;

                    this.updateVisibleState.complete = false;
                    this.updateAllowImageEnhancements.complete = false;

                    //this.updateName.processing = false;
                    this.updateMaxUsers.processing = false;
                    this.updateMaxCategories.processing = false;
                    this.updateMaxSubcategories.processing = false;
                    this.updateMaxTags.processing = false;
                    this.updateMaxImages.processing = false;

                    this.updateVisibleState.processing = false;
                    this.updateAllowImageEnhancements.processing = false;
                },



            }


        vm.changePlanVisibleState = function (planName) {

            vm.planDetail.updateVisibleState.processing = true;
            var previousValue = vm.planDetail.Visible;
            vm.planDetail.Visible = null;

            plansServices.updatePlanVisibility(planName, !previousValue)
              .success(function (data, status, headers, config) {

                  vm.planDetail.updateVisibleState.processing = false;
                  vm.planDetail.updateVisibleState.complete = true;

                  if (data.isSuccess) {

                      vm.planDetail.Visible = !previousValue;

                      //refresh plans
                      vm.refreshPaymentPlans();

                      vm.planDetail.updateVisibleState.isSuccess = true;
                      vm.planDetail.updateVisibleState.message = "Visible state updated!";
                  } else {

                      vm.planDetail.Visible = previousValue;
                      vm.planDetail.updateVisibleState.isSuccess = false;
                      vm.planDetail.updateVisibleState.message = data.ErrorMessage;
                  }
              })
              .error(function (data, status, headers, config) {
                  vm.planDetail.updateVisibleState.processing = false;
                  vm.planDetail.Visible = previousValue;
                  vm.planDetail.updateVisibleState.isSuccess = false;
                  vm.planDetail.updateVisibleState.complete = true;
                  vm.planDetail.updateVisibleState.message = "An error occured while attempting to use the service!";
              });
        };

        vm.resetUpdatePlanVisibleStateResult = function () {
            Debug.trace("Visibility complete...");
            vm.planDetail.updateVisibleState.complete = false;
        };

        //private method

        function resetPlanDetailModalProperties() {
            vm.planDetail.reset();
            vm.planDeletion.reset();
        }



        // ---------- Update Plan MaxUsers ------------

        vm.editPlanMaxUsers = function () {
            vm.planDetail.updateMaxUsers.edit = true;
            editingPlan.newMaxUsers = angular.copy(vm.planDetail.MaxUsers);
        };

        vm.cancelPlanMaxUsers = function () {
            vm.planDetail.reset();
            vm.planDetail.MaxUsers = editingPlan.newMaxUsers;
        };


        vm.updatePlanMaxUsers = function (planName) {

            vm.planDetail.updateMaxUsers.edit = false;
            vm.planDetail.updateMaxUsers.processing = true;


            plansServices.updatePlanMaxUsers(planName, vm.planDetail.MaxUsers)
                .success(function (data, status, headers, config) {

                    vm.planDetail.updateMaxUsers.processing = false;
                    vm.planDetail.updateMaxUsers.complete = true;

                    if (data.isSuccess) {

                        //refresh plans
                        vm.refreshPaymentPlans();

                        vm.planDetail.updateMaxUsers.isSuccess = true;
                        vm.planDetail.updateMaxUsers.message = "MaxUsers updated!";

                    } else {

                        vm.planDetail.updateMaxUsers.isSuccess = false;
                        vm.planDetail.updateMaxUsers.message = data.ErrorMessage;
                    }

                })
                .error(function (data, status, headers, config) {

                    vm.planDetail.updateMaxUsers.processing = false;

                    vm.planDetail.updateMaxUsers.isSuccess = false;
                    vm.planDetail.updateMaxUsers.complete = true;
                    vm.planDetail.updateMaxUsers.message = "An error occured while attempting to use the service!";
                });
        };

        vm.resetUpdatePlanMaxUsersResult = function () {
            if (!vm.planDetail.updateMaxUsers.isSuccess) {
                vm.planDetail.updateMaxUsers.complete = false;
                vm.planDetail.updateMaxUsers.edit = true;
            }
            else {
                vm.planDetail.reset();
            }

        };

        // ---------- Update Plan MaxCategories ------------

        vm.editPlanMaxCategories = function () {
            vm.planDetail.updateMaxCategories.edit = true;
            editingPlan.newMaxCategories = angular.copy(vm.planDetail.MaxCategories);
        };

        vm.cancelPlanMaxCategories = function () {
            vm.planDetail.reset();
            vm.planDetail.MaxCategories = editingPlan.newMaxCategories;
        };


        vm.updatePlanMaxCategories = function (planName) {

            vm.planDetail.updateMaxCategories.edit = false;
            vm.planDetail.updateMaxCategories.processing = true;


            plansServices.updatePlanMaxCategories(planName, vm.planDetail.MaxCategories)
                .success(function (data, status, headers, config) {

                    vm.planDetail.updateMaxCategories.processing = false;
                    vm.planDetail.updateMaxCategories.complete = true;

                    if (data.isSuccess) {

                        //refresh plans
                        vm.refreshPaymentPlans();

                        vm.planDetail.updateMaxCategories.isSuccess = true;
                        vm.planDetail.updateMaxCategories.message = "MaxCategories updated!";

                    } else {

                        vm.planDetail.updateMaxCategories.isSuccess = false;
                        vm.planDetail.updateMaxCategories.message = data.ErrorMessage;
                    }

                })
                .error(function (data, status, headers, config) {

                    vm.planDetail.updateMaxCategories.processing = false;

                    vm.planDetail.updateMaxCategories.isSuccess = false;
                    vm.planDetail.updateMaxCategories.complete = true;
                    vm.planDetail.updateMaxCategories.message = "An error occured while attempting to use the service!";
                });
        };

        vm.resetUpdatePlanMaxCategoriesResult = function () {
            if (!vm.planDetail.updateMaxCategories.isSuccess) {
                vm.planDetail.updateMaxCategories.complete = false;
                vm.planDetail.updateMaxCategories.edit = true;
            }
            else {
                vm.planDetail.reset();
            }

        };

        // ---------- Update Plan MaxSubcategories ------------

        vm.editPlanMaxSubcategories = function () {
            vm.planDetail.updateMaxSubcategories.edit = true;
            editingPlan.newMaxSubcategories = angular.copy(vm.planDetail.MaxSubcategories);
        };

        vm.cancelPlanMaxSubcategories = function () {
            vm.planDetail.reset();
            vm.planDetail.MaxSubcategories = editingPlan.newMaxSubcategories;
        };


        vm.updatePlanMaxSubcategories = function (planName) {

            vm.planDetail.updateMaxSubcategories.edit = false;
            vm.planDetail.updateMaxSubcategories.processing = true;


            plansServices.updatePlanMaxSubcategories(planName, vm.planDetail.MaxSubcategories)
                .success(function (data, status, headers, config) {

                    vm.planDetail.updateMaxSubcategories.processing = false;
                    vm.planDetail.updateMaxSubcategories.complete = true;

                    if (data.isSuccess) {

                        //refresh plans
                        vm.refreshPaymentPlans();

                        vm.planDetail.updateMaxSubcategories.isSuccess = true;
                        vm.planDetail.updateMaxSubcategories.message = "MaxSubcategories updated!";

                    } else {

                        vm.planDetail.updateMaxSubcategories.isSuccess = false;
                        vm.planDetail.updateMaxSubcategories.message = data.ErrorMessage;
                    }

                })
                .error(function (data, status, headers, config) {

                    vm.planDetail.updateMaxSubcategories.processing = false;

                    vm.planDetail.updateMaxSubcategories.isSuccess = false;
                    vm.planDetail.updateMaxSubcategories.complete = true;
                    vm.planDetail.updateMaxSubcategories.message = "An error occured while attempting to use the service!";
                });
        };

        vm.resetUpdatePlanMaxSubcategoriesResult = function () {
            if (!vm.planDetail.updateMaxSubcategories.isSuccess) {
                vm.planDetail.updateMaxSubcategories.complete = false;
                vm.planDetail.updateMaxSubcategories.edit = true;
            }
            else {
                vm.planDetail.reset();
            }

        };

        // ---------- Update Plan MaxTags ------------

        vm.editPlanMaxTags = function () {
            vm.planDetail.updateMaxTags.edit = true;
            editingPlan.newMaxTags = angular.copy(vm.planDetail.MaxTags);
        };

        vm.cancelPlanMaxTags = function () {
            vm.planDetail.reset();
            vm.planDetail.MaxTags = editingPlan.newMaxTags;
        };


        vm.updatePlanMaxTags = function (planName) {

            vm.planDetail.updateMaxTags.edit = false;
            vm.planDetail.updateMaxTags.processing = true;


            plansServices.updatePlanMaxTags(planName, vm.planDetail.MaxTags)
                .success(function (data, status, headers, config) {

                    vm.planDetail.updateMaxTags.processing = false;
                    vm.planDetail.updateMaxTags.complete = true;

                    if (data.isSuccess) {

                        //refresh plans
                        vm.refreshPaymentPlans();

                        vm.planDetail.updateMaxTags.isSuccess = true;
                        vm.planDetail.updateMaxTags.message = "MaxTags updated!";

                    } else {

                        vm.planDetail.updateMaxTags.isSuccess = false;
                        vm.planDetail.updateMaxTags.message = data.ErrorMessage;
                    }

                })
                .error(function (data, status, headers, config) {

                    vm.planDetail.updateMaxTags.processing = false;

                    vm.planDetail.updateMaxTags.isSuccess = false;
                    vm.planDetail.updateMaxTags.complete = true;
                    vm.planDetail.updateMaxTags.message = "An error occured while attempting to use the service!";
                });
        };

        vm.resetUpdatePlanMaxTagsResult = function () {
            if (!vm.planDetail.updateMaxTags.isSuccess) {
                vm.planDetail.updateMaxTags.complete = false;
                vm.planDetail.updateMaxTags.edit = true;
            }
            else {
                vm.planDetail.reset();
            }

        };

        // ---------- Update Plan MaxImages ------------

        vm.editPlanMaxImages = function () {
            vm.planDetail.updateMaxImages.edit = true;
            editingPlan.newMaxImages = angular.copy(vm.planDetail.MaxImages);
        };

        vm.cancelPlanMaxImages = function () {
            vm.planDetail.reset();
            vm.planDetail.MaxImages = editingPlan.newMaxImages;
        };


        vm.updatePlanMaxImages = function (planName) {

            vm.planDetail.updateMaxImages.edit = false;
            vm.planDetail.updateMaxImages.processing = true;


            plansServices.updatePlanMaxImages(planName, vm.planDetail.MaxImages)
                .success(function (data, status, headers, config) {

                    vm.planDetail.updateMaxImages.processing = false;
                    vm.planDetail.updateMaxImages.complete = true;

                    if (data.isSuccess) {

                        //refresh plans
                        vm.refreshPaymentPlans();

                        vm.planDetail.updateMaxImages.isSuccess = true;
                        vm.planDetail.updateMaxImages.message = "MaxImages updated!";

                    } else {

                        vm.planDetail.updateMaxImages.isSuccess = false;
                        vm.planDetail.updateMaxImages.message = data.ErrorMessage;
                    }

                })
                .error(function (data, status, headers, config) {

                    vm.planDetail.updateMaxImages.processing = false;

                    vm.planDetail.updateMaxImages.isSuccess = false;
                    vm.planDetail.updateMaxImages.complete = true;
                    vm.planDetail.updateMaxImages.message = "An error occured while attempting to use the service!";
                });
        };

        vm.resetUpdatePlanMaxImagesResult = function () {
            if (!vm.planDetail.updateMaxImages.isSuccess) {
                vm.planDetail.updateMaxImages.complete = false;
                vm.planDetail.updateMaxImages.edit = true;
            }
            else {
                vm.planDetail.reset();
            }

        };


        // - Update Allow Image Enhancements on a Plan ------------------------------

        vm.changePlanAllowImageEnhancements = function (planName) {

            vm.planDetail.updateAllowImageEnhancements.processing = true;
            var previousValue = vm.planDetail.AllowImageEnhancements;
            vm.planDetail.AllowImageEnhancements = null;

            plansServices.updatePlanAllowImageEnhancements(planName, !previousValue)
              .success(function (data, status, headers, config) {

                  vm.planDetail.updateAllowImageEnhancements.processing = false;
                  vm.planDetail.updateAllowImageEnhancements.complete = true;

                  if (data.isSuccess) {

                      vm.planDetail.AllowImageEnhancements = !previousValue;

                      //refresh plans
                      vm.refreshPaymentPlans();

                      vm.planDetail.updateAllowImageEnhancements.isSuccess = true;
                      vm.planDetail.updateAllowImageEnhancements.message = "Enhancement allowance updated!";
                  } else {
                      vm.planDetail.AllowImageEnhancements = previousValue;
                      vm.planDetail.updateAllowImageEnhancements.isSuccess = false;
                      vm.planDetail.updateAllowImageEnhancements.message = data.ErrorMessage;
                  }
              })
              .error(function (data, status, headers, config) {
                  vm.planDetail.updateAllowImageEnhancements.processing = false;
                  vm.planDetail.AllowImageEnhancements = previousValue;
                  vm.planDetail.updateAllowImageEnhancements.isSuccess = false;
                  vm.planDetail.updateAllowImageEnhancements.complete = true;
                  vm.planDetail.updateAllowImageEnhancements.message = "An error occured while attempting to use the service!";
              });
        };

        vm.resetUpdatePlanAllowImageEnhancementsResult = function () {
            Debug.trace("Enhancements allowance complete...");
            vm.planDetail.updateAllowImageEnhancements.complete = false;
        };

        /* ==========================================
                   DELETE PLAN
        ==========================================*/

        vm.planDeletion =
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

        vm.startPlanDeletion = function () {
            vm.planDeletion.Verify = true;
        }

        vm.cancelPlanDeletion = function () {
            vm.planDeletion.reset();
        }

        vm.deletePlan = function (planName) {

            vm.planDeletion.Verify = false;
            vm.planDeletion.Processing = true;

            Debug.trace("Deleting plan...");

            plansServices.deletePlan(planName)
            .success(function (data, status, headers, config) {

                vm.planDeletion.Processing = false;
                vm.planDeletion.Complete = true;

                if (data.isSuccess) {

                    //refresh plans
                    vm.refreshPaymentPlans();

                    vm.planDeletion.IsSuccess = true;
                    vm.planDeletion.Message = "This plan has been deleted.";
                    //vm.planDetail.FullName = null;

                }
                else {

                    vm.planDeletion.IsSuccess = false;
                    vm.planDeletion.Message = data.ErrorMessage;
                }

            })
                .error(function (data, status, headers, config) {

                    vm.planDeletion.Processing = false;
                    vm.planDetailUpdates.IsSuccess = false;
                    vm.planDetailUpdates.Complete = true;
                    vm.planDetailUpdates.Message = "An error occurred while attempting to use the service...";

                })


        }

        /* ==========================================
           CREATE Plan MODAL
       ==========================================*/


        vm.newPlan =
            {
                //DocumentPartitionTierID: null,

                Name: null,
                MonthlyRate: null,

                MaxUsers: null,
                MaxCategorizationsPerSet: null,
                MaxProductsPerSet: null,
                MaxProperties: null,
                MaxValuesPerProperty: null,
                MaxTags: null,

                AllowSalesLeads: false,
                AllowLocationData: false,
                AllowCustomOrdering: false,
                AllowThemes: false,

                MaxImageGroups: null,
                MaxImageFormats: null,
                MaxImageGalleries: null,
                MaxImagesPerGallery: null,

                AllowImageEnhancements: false,

                BasicSupport: true,
                EnhancedSupport: false,
                Visible: false,


                // Service Processing --------

                IsSending: false,
                SendingComplete: false,

                Results: {
                    IsSuccess: false,
                    Message: null
                },

                SetThemes: function (allow) {
                    this.AllowThemes = allow;
                },

                SetLocationData: function (allow) {
                    this.AllowLocationData = allow;
                },

                SetCustomOrdering: function (allow) {
                    this.AllowCustomOrdering = allow;
                },
                
                SetSalesLeads: function (allow) {
                    this.AllowSalesLeads = allow;
                },

                SetEnhancements: function(allow)
                {
                    this.AllowImageEnhancements = allow;
                },

                SetBasicSupport: function (allow) {
                    this.BasicSupport = allow;
                },
                SetEnhancedSupport: function (allow) {
                    this.EnhancedSupport = allow;
                },
                SetVisibility: function(visible)
                {
                    this.Visible = visible;
                },

                // Cleanup Routine(s) ----------

                Retry: function () {
                    this.IsSending = false;
                    this.SendingComplete = false;
                },

                Clear: function () {
                    Debug.trace("Clearing new plan form data.");

                    if(vm.documentPartitionTiers == null)
                    {
                        vm.getDocumentPartitionTiers();
                    }
                    else
                    {
                        vm.currentDocumentPartitionTier = vm.documentPartitionTiers[0];
                    }

                    //this.DocumentPartitionTierID = null,
                    this.Name = null;
                    this.MonthlyRate = null;
                    this.MaxUsers = null;
                    this.MaxCategorizationsPerSet = null;
                    this.MaxProductsPerSet = null;
                    this.MaxProperties = null;
                    this.MaxValuesPerProperty = null;
                    this.MaxTags = null;

                    this.AllowSalesLeads = false;
                    this.AllowLocationData = false;
                    this.AllowCustomOrdering = false;
                    this.AllowThemes = false;

                    this.AllowImageEnhancements = false;

                    this.BasicSupport = true;
                    this.EnhancedSupport = false;
                    this.Visible = false;

                    this.MaxImageGroups = null
                    this.MaxImageFormats = null;
                    this.MaxImageGalleries = null;
                    this.MaxImagesPerGallery = null;

                    this.IsSending = false;
                    this.SendingComplete = false;

                    this.Results.IsSuccess = false;
                    this.Results.Message = null;

                }
            }


        vm.createPlan = function () {
            vm.newPlan.IsSending = true;

            Debug.trace("Creating Plan...");

            plansServices.createPlan(vm.newPlan.Name, vm.newPlan.MonthlyRate, vm.newPlan.MaxUsers, vm.newPlan.MaxCategorizationsPerSet, vm.newPlan.MaxProductsPerSet,
                vm.newPlan.MaxProperties, vm.newPlan.MaxValuesPerProperty, vm.newPlan.MaxTags, vm.newPlan.AllowSalesLeads, vm.newPlan.AllowLocationData, vm.newPlan.AllowCustomOrdering, vm.newPlan.AllowThemes, vm.newPlan.AllowImageEnhancements, vm.newPlan.BasicSupport, vm.newPlan.EnhancedSupport, vm.newPlan.MaxImageGroups, vm.newPlan.MaxImageFormats, vm.newPlan.MaxImageGalleries, vm.newPlan.MaxImagesPerGallery, vm.newPlan.Visible)
            .success(function (data, status, headers, config) {

                vm.newPlan.IsSending = false;
                vm.newPlan.SendingComplete = true;

                if (data.isSuccess) {

                    vm.newPlan.Results.IsSuccess = true;
                    vm.newPlan.Results.Message = "Plan created!";
                    
                    //refresh plans
                    vm.refreshPaymentPlans();
                }
                else {
                    vm.newPlan.Results.IsSuccess = false;
                    vm.newPlan.Results.Message = data.ErrorMessage;
                }

            })
                .error(function (data, status, headers, config) {

                    vm.newPlan.Results.IsSuccess = false;

                    vm.newPlan.IsSending = false;
                    vm.newPlan.SendingComplete = true;
                    vm.newPlan.Results.Message = "An error occurred while attempting to use the service...";
                })
        }



        /* ====================================================

           Manage Document Partition Tiers

        =======================================================*/

        vm.updateCurrentDocumentPartitionTier = function(documentPartitionTier)
        {
            vm.currentDocumentPartitionTier = documentPartitionTier;
        }

        vm.getDocumentPartitionTiers = function () {
            

            platformServices.getDocumentPartitionTiers()
                        .success(function (data, status, headers, config) {

                            vm.documentPartitionTiers = data;
                            vm.currentDocumentPartitionTier = vm.documentPartitionTiers[0];

                        })
                        .error(function (data, status, headers, config) {

                        })


        }









        /* =========================================================================================


           End Payment Plan Panel Directive


       =============================================================================================*/






        // -------- SHARED -------------------------------------------

        vm.goToAccount = function (accountNameKey) {
            window.location.href = 'account/' + accountNameKey;
        }






        /* ===========================================================================



            START PAYMENTS/CHARGES PANEL



        ========================================================================= */



        // - Payment/Charge Variables -------------

        vm.showAccountColumnForPayments = true;
        vm.paymentsPerPage = 10;
        vm.payments_next = [];
        vm.payments_last = [];


        //---------------------------------------------------

        vm.activatePaymentHistoryPanel = function () {
            Debug.trace("Activating payment panel... ");

            vm.getPaymentHistory();

        }

        //---------------------------------------------------


        //---------------------------------------------------

        vm.getPaymentHistory = function () {

            vm.payments = [];
            vm.payments_next = [];
            vm.payments_last = [];

            vm.paymentHistoryPanelLoading = true;

            Debug.trace("Getting payment transactions (initial list)... ");

            paymentServices.getPaymentHistory(vm.paymentsPerPage, null)
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

        // ---------------------------------------------------

        vm.preload_NextPayments = function () {
            vm.paymentsPreloadingNext = true;

            vm.payments_next = [];
            Debug.trace("Preloading next payments... ");


            paymentServices.getPaymentHistory_Next(vm.paymentsPerPage, vm.payments[vm.payments.length - 1].ChargeID, null)
                        .success(function (data, status, headers, config) {

                            vm.paymentsPreloadingNext = false;
                            vm.payments_next = data;
                        })
                        .error(function (data, status, headers, config) {

                        })


        }

        vm.getPaymentHistory_Next = function () {

            Debug.trace("Getting next payments... ");

            vm.payments_last = vm.payments
            vm.payments = vm.payments_next
            vm.preload_NextPayments();
        }

        //---------------------------------------------------

        vm.preload_LastPayments = function () {

            vm.paymentsPreloadingLast = true;

            vm.payments_last = [];

            Debug.trace("Preloading last payments... ");

            Debug.trace(vm.payments);


           paymentServices.getPaymentHistory_Last(vm.paymentsPerPage, vm.payments[0].ChargeID, null)
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

        /* ===========================================================================



            END PAYMENTS/CHARGES PANEL



        ========================================================================= */


        /* ==============================================

    PAYMENT/CHARGE DETAILS

    ============================================*/

        vm.loadingPaymentDetails = false;
        vm.showRefundsTab = true;

        vm.getPaymentDetail = function (index, reset) {
            vm.refundPayment.reset();
            vm.showRefundsTab = true;

            
            //vm.refundPayment.Index = index;

            vm.paymentDetailsDefaultTabActive = true;

            // if (reset == true) {
            //Debug.trace("Resetting details...");
            //vm.paymentDetail.reset();
            //}


            vm.paymentDetailBalances = null
            vm.loadingPaymentDetails = true;

            vm.paymentDetail = vm.payments[index];
            vm.paymentDetail.Index = index;

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

                AccountID: null,
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

                    this.AccountID = null,
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


        vm.startPaymentRefund = function () {
            vm.refundPayment.reset();
            vm.refundPayment.update.edit = true;

            vm.refundPayment.RefundAmount = (vm.paymentDetail.Amount - vm.paymentDetail.TotalRefunded).toFixed(2);
            vm.refundPayment.ChargeID = vm.paymentDetail.ChargeID;
            vm.refundPayment.AccountID = vm.paymentDetail.AccountID;

            //Debug.trace(vm.refundPayment.RefundAmount + "|" + vm.paymentDetail.Amount);

        }
        vm.cancelRefundPayment = function () {
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
        vm.applyRefund = function () {
            if (vm.isNumber(vm.refundPayment.RefundAmount) == false) {
                vm.refundPayment.update.edit = false;
                vm.refundPayment.update.complete = true;

                vm.refundPayment.update.isSuccess = false;
                vm.refundPayment.update.message = "Please use a monetary value with 2 decimal places. Examples: '2.25' or '.50'";
            }
            else {
                vm.refundPayment.update.edit = false;
                vm.refundPayment.update.processing = true;

                paymentServices.refundPayment(vm.refundPayment.AccountID, vm.refundPayment.ChargeID, vm.refundPayment.RefundAmount)
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

        vm.isNumber = function (n) {
            return !isNaN(parseFloat(n)) && isFinite(n);
        }


        //------------------------------------------------------------------

        //used to refresh latest list in the grid when making payment detail updates (refunds, etc...)
        vm.refreshPaymentHistoryListAndDetails = function () {
            //vm.paymentHistoryPanelLoading = true;



            //check if we are on the first page of the list, or deeper along:
            if (vm.payments_last.length > 0) {
                //if we are further along use the ChargeID of the last item in vm.payments_last

                paymentServices.getPaymentHistory_Next(vm.paymentsPerPage, vm.payments_last[vm.payments_last.length - 1].ChargeID)
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
            else {
                //Otherwise we call the initial histrical list
                paymentServices.getPaymentHistory(vm.paymentsPerPage, null)
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















        /* ===========================================================================



            START INVOICE PANEL



        ========================================================================= */




        // - Invoice Variables -------------

        vm.invoicesPerPage = 10;
        vm.invoices_next = [];
        vm.invoices_last = [];


        vm.showingInvoiceHistoryDateRange = false;
        vm.invoice_daterange_start = null;
        vm.invoice_daterange_end = null;


        //--------------------------------------------------




        //---------------------------------------------------

        vm.activateInvoiceHistoryPanel = function ()
        {
            Debug.trace("Activating invoice panel... ");

            vm.getInvoiceHistory();

            //Initialize Date Range Picker(s)
            $('.input-daterange').datepicker({
                //todayHighlight: true
            });
        }

        //---------------------------------------------------

        vm.clearInvoiceDateRange = function () {
            vm.invoice_daterange_start = null;
            vm.invoice_daterange_end = null;

            vm.showingInvoiceHistoryDateRange = false;

        }


        //---------------------------------------------------
        /*
        vm.getNextInvoice = function () {

            Debug.trace("Getting upcoming invoice... ");

            invoiceServices.getNextInvoice()
                        .success(function (data, status, headers, config) {

                            //vm.account.NextInvoice = data;

                        })
                        .error(function (data, status, headers, config) {
                            //
                        })
        }
        */
        //---------------------------------------------------

        vm.getInvoiceHistory = function () {

            vm.clearInvoiceDateRange();
            vm.invoices = [];
            vm.invoices_next = [];
            vm.invoices_last = [];

            vm.invoiceHistoryPanelLoading = true;

            Debug.trace("Getting invoice transactions (initial list)... ");

            invoiceServices.getInvoiceHistory(vm.invoicesPerPage, null)
                        .success(function (data, status, headers, config) {

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

        // ---------------------------------------------------

        vm.filterInvoicesByDateRange = function () {

            vm.invoices = [];
            vm.invoices_next = [];
            vm.invoices_last = [];

            vm.invoiceHistoryPanelLoading = true;

            Debug.trace("Getting invoice transactions (by date range)... ");

            invoiceServices.getInvoiceHistory_ByDateRange(vm.invoicesPerPage, vm.invoice_daterange_start, vm.invoice_daterange_end, null)
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

        vm.preload_NextInvoices = function () {
            vm.invoicesPreloadingNext = true;

            vm.invoices_next = [];
            Debug.trace("Preloading next invoices... ");


            if (vm.invoice_daterange_start == null && vm.invoice_daterange_end == null) {
                invoiceServices.getInvoiceHistory_Next(vm.invoicesPerPage, vm.invoices[vm.invoices.length - 1].InvoiceID, null)
                            .success(function (data, status, headers, config) {

                                vm.invoicesPreloadingNext = false;
                                vm.invoices_next = data;
                            })
                            .error(function (data, status, headers, config) {

                            })
            }
            else {
                invoiceServices.getInvoiceHistory_ByDateRange_Next(vm.invoicesPerPage, vm.invoice_daterange_start, vm.invoice_daterange_end, vm.invoices[vm.invoices.length - 1].InvoiceID, null)
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

                invoiceServices.getInvoiceHistory_Last(vm.invoicesPerPage, vm.invoices[0].InvoiceID, null)
                            .success(function (data, status, headers, config) {

                                vm.invoicesPreloadingLast = false;
                                vm.invoices_last = data;

                            })
                            .error(function (data, status, headers, config) {

                            })
            }
            else {
                invoiceServices.getInvoiceHistory_ByDateRange_Last(vm.invoicesPerPage, vm.invoice_daterange_start, vm.invoice_daterange_end, vm.invoices[0].InvoiceID, null)
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

        /* ===========================================================================



            END INVOICE PANEL



        ========================================================================= */

        /* ==============================================

        INVOICE DETAILS

        ============================================*/

        vm.loadingInvoiceDetails = false;

        vm.getInvoiceDetail = function (index, reset) {


            vm.invoiceDetail = vm.invoices[index];
            vm.invoiceDetail.Index = index;

        }








        /* ===========================================================================



            START TRANSFERS PANEL



        ========================================================================= */




        // - Transfer Variables -------------

        vm.transfersPerPage = 10;
        vm.transfers_next = [];
        vm.transfers_last = [];


        vm.showingTransferHistoryDateRange = false;
        vm.transfer_daterange_start = null;
        vm.transfer_daterange_end = null;


        //--------------------------------------------------




        //---------------------------------------------------

        vm.activateTransferHistoryPanel = function () {
            Debug.trace("Activating transfer panel... ");

            vm.getTransferHistory();

            //Initialize Date Range Picker(s)
            $('.input-daterange').datepicker({
                //todayHighlight: true
            });
        }

        //---------------------------------------------------

        vm.clearTransferDateRange = function () {
            vm.transfer_daterange_start = null;
            vm.transfer_daterange_end = null;

            vm.showingTransferHistoryDateRange = false;

        }

        //---------------------------------------------------

        vm.getTransferHistory = function () {

            vm.clearTransferDateRange();
            vm.transfers = [];
            vm.transfers_next = [];
            vm.transfers_last = [];

            vm.transferHistoryPanelLoading = true;

            Debug.trace("Getting transfer transactions (initial list)... ");

            transferServices.getTransferHistory(vm.transfersPerPage)
                        .success(function (data, status, headers, config) {

                            vm.transferHistoryPanelLoading = false;
                            vm.transfers = data;

                            //vm.TransferIDIndex1 = vm.transfers[0].TransferID

                            if (vm.transfers.length == vm.transfersPerPage) {
                                vm.preload_NextTransfers();
                            }

                        })
                        .error(function (data, status, headers, config) {
                            vm.transferHistoryPanelLoading = false;
                        })
        }

        // ---------------------------------------------------

        vm.filterTransfersByDateRange = function () {

            vm.transfers = [];
            vm.transfers_next = [];
            vm.transfers_last = [];

            vm.transferHistoryPanelLoading = true;

            Debug.trace("Getting transfer transactions (by date range)... ");

            transferServices.getTransferHistory_ByDateRange(vm.transfersPerPage, vm.transfer_daterange_start, vm.transfer_daterange_end)
                        .success(function (data, status, headers, config) {

                            vm.showingTransferHistoryDateRange = true;

                            vm.transferHistoryPanelLoading = false;
                            vm.transfers = data;

                            //vm.TransferIDIndex1 = vm.transfers[0].TransferID

                            if (vm.transfers.length == vm.transfersPerPage) {
                                vm.preload_NextTransfers();
                            }

                        })
                        .error(function (data, status, headers, config) {
                            vm.transferHistoryPanelLoading = false;
                        })

        }

        //---------------------------------------------------

        vm.preload_NextTransfers = function () {
            vm.transfersPreloadingNext = true;

            vm.transfers_next = [];
            Debug.trace("Preloading next transfers... ");


            if (vm.transfer_daterange_start == null && vm.transfer_daterange_end == null) {
                transferServices.getTransferHistory_Next(vm.transfersPerPage, vm.transfers[vm.transfers.length - 1].TransferID)
                            .success(function (data, status, headers, config) {

                                vm.transfersPreloadingNext = false;
                                vm.transfers_next = data;
                            })
                            .error(function (data, status, headers, config) {

                            })
            }
            else {
                transferServices.getTransferHistory_ByDateRange_Next(vm.transfersPerPage, vm.transfer_daterange_start, vm.transfer_daterange_end, vm.transfers[vm.transfers.length - 1].TransferID)
                            .success(function (data, status, headers, config) {

                                vm.transfersPreloadingNext = false;
                                vm.transfers_next = data;
                            })
                            .error(function (data, status, headers, config) {

                            })
            }

        }

        vm.getTransferHistory_Next = function () {

            Debug.trace("Getting next transfers... ");

            vm.transfers_last = vm.transfers
            vm.transfers = vm.transfers_next
            vm.preload_NextTransfers();
        }

        //---------------------------------------------------

        vm.preload_LastTransfers = function () {

            vm.transfersPreloadingLast = true;

            vm.transfers_last = [];

            Debug.trace("Preloading last transfers... ");

            Debug.trace(vm.transfers);

            if (vm.transfer_daterange_start == null && vm.transfer_daterange_end == null) {

                transferServices.getTransferHistory_Last(vm.transfersPerPage, vm.transfers[0].TransferID)
                            .success(function (data, status, headers, config) {

                                vm.transfersPreloadingLast = false;
                                vm.transfers_last = data;

                            })
                            .error(function (data, status, headers, config) {

                            })
            }
            else {
                transferServices.getTransferHistory_ByDateRange_Last(vm.transfersPerPage, vm.transfer_daterange_start, vm.transfer_daterange_end, vm.transfers[0].TransferID)
                            .success(function (data, status, headers, config) {

                                vm.transfersPreloadingLast = false;
                                vm.transfers_last = data;

                            })
                            .error(function (data, status, headers, config) {

                            })
            }

        }


        vm.getTransferHistory_Last = function () {

            Debug.trace("Getting last transfers... ");

            vm.transfers_next = vm.transfers;
            vm.transfers = vm.transfers_last;
            vm.preload_LastTransfers();
        }

        /* ===========================================================================



            END TRANSFERS PANEL



        ========================================================================= */











        /* ==========================================
           CURRENT USER PROFILE
       ==========================================*/

        function updateCurrentUserProfile() {

            Debug.trace("Refreshing user profile...");

            sharedServices.getCurrentUserProfile()
            .success(function (data, status, headers, config) {

                vm.currentUserProfile = data; //Used to determine what is shown in the view based on user Role.
                currentUserRoleIndex = vm.platformRoles.indexOf(data.Role) //<-- use PLATFORM roles, NOT ACCOUNT roles!

                Debug.trace("Profile refreshed!");
                Debug.trace("Role index = " + currentUserRoleIndex);

            })
                .error(function (data, status, headers, config) {


                })

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

            //Platform Roles (used for the logged in Platform user, to check Roles accesability
            vm.platformRoles = JSON.parse(CoreServiceSettings_PlatformUsers_RolesList);

            // Load local profile for the platfor user.
            vm.currentUserProfile = JSON.parse(CurrentUserProfile);
            currentUserRoleIndex = vm.platformRoles.indexOf(vm.currentUserProfile.Role) //<-- Role will indicate what editing capabilites are available.
            // Refresh the profile every 45 seconds (if Role is updated, new editing capabilites will light up for the user)
            setInterval(function () { updateCurrentUserProfile() }, 150000);


            //vm.activatePaymentPlansPanel();

            vm.getSnapshot();
            vm.getReport();

            Debug.trace('billingIndexController activation complete');



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

