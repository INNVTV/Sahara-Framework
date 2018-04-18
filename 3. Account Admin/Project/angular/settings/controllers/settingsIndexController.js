(function () {
    'use strict';

    var controllerId = 'settingsIndexController';

    // Inject into main app module:
    angular.module('app')
        .controller(controllerId, [
            'propertiesServices',
            'tagServices',
            'imageFormatServices',
            'sharedServices',
            'accountServices',
            'accountSettingsServices',
            'leadsServices',
            settingsIndexController
    ]);

    function settingsIndexController(propertiesServices, tagServices, imageFormatServices, sharedServices, accountServices, accountSettingsServices, leadsServices) {

        //Instantiate Controller as ViewModel
        var vm = this;

        //Default Properties: =============================
        vm.title = 'settingsIndexController';
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

       
        vm.showLoader = true;

        /* ==========================================
               Base Controller Methods
        ==========================================*/

        vm.account = null;

        vm.getAccount = function () {

            accountServices.getAccount()
                    .success(function (data, status, headers, config) {
                        vm.account = data;
                        vm.getAccountSettings();
                        vm.showLoader = false;
                        
                    })
                    .error(function (data, status, headers, config) {

                    })
        }




        /*==========================================
              General Settings
         ==========================================*/

        vm.accountSettings = null;
        
        vm.getAccountSettings = function () {

            //Debug.trace("Getting tags...");

            accountSettingsServices.getAccountSettings()
            .success(function (data, status, headers, config) {

                vm.accountSettings = data;
                
                //vm.getThemes(); //<--No longer used

            })
            .error(function (data, status, headers, config) {

            })
        }

        //Themes -------

        vm.themes = null;
        vm.getThemes = function () {

            accountSettingsServices.getThemes()
            .success(function (data, status, headers, config) {

                vm.themes = data;

            })
            .error(function (data, status, headers, config) {

            })
        }


        vm.updateAccountSettingContactShowPhoneNumber = function(showPhoneNumber)
        {
            var previousState = vm.accountSettings.ContactSettings.ShowPhoneNumber;
            vm.accountSettings.ContactSettings.ShowPhoneNumber = null;

            accountSettingsServices.updateShowPhoneNumber(showPhoneNumber)
            .success(function (data, status, headers, config) {

                if (data.isSuccess) {
                    vm.accountSettings.ContactSettings.ShowPhoneNumber = showPhoneNumber;
                }
                else {
                    vm.accountSettings.ContactSettings.ShowPhoneNumber = previousState;
                }
            })
                .error(function (data, status, headers, config) {
                    vm.accountSettings.ContactSettings.ShowPhoneNumber = previousState;
                })
        }

        vm.updateAccountSettingContactShowAddress = function (showAddress) {
            var previousState = vm.accountSettings.ContactSettings.ShowAddress;
            vm.accountSettings.ContactSettings.ShowAddress = null;

            accountSettingsServices.updateShowAddress(showAddress)
            .success(function (data, status, headers, config) {

                if (data.isSuccess) {
                    vm.accountSettings.ContactSettings.ShowAddress = showAddress;
                }
                else {
                    vm.accountSettings.ContactSettings.ShowAddress = previousState;
                }
            })
                .error(function (data, status, headers, config) {
                    vm.accountSettings.ContactSettings.ShowAddress = previousState;
                })
        }

        vm.updateAccountSettingContactShowEmail = function (showEmail) {
            var previousState = vm.accountSettings.ContactSettings.ShowEmail;
            vm.accountSettings.ContactSettings.ShowEmail = null;

            accountSettingsServices.updateShowEmail(showEmail)
            .success(function (data, status, headers, config) {

                if (data.isSuccess) {
                    vm.accountSettings.ContactSettings.ShowEmail = showEmail;
                }
                else {
                    vm.accountSettings.ContactSettings.ShowEmail = previousState;
                }
            })
                .error(function (data, status, headers, config) {
                    vm.accountSettings.ContactSettings.ShowEmail = previousState;
                })
        }

        /*==========================================
              Theme Settings
         ==========================================*/

        vm.updateAccountTheme = function (theme) {

            var previousState = vm.accountSettings.Theme;
            vm.accountSettings.Theme = theme;

            $("#themeUpdating").show();

            accountSettingsServices.updateAccountTheme(theme)
            .success(function (data, status, headers, config) {

                $("#themeUpdating").hide();

                if (data.isSuccess) {
                    vm.accountSettings.Theme = theme;
                    $("#themeSuccess").fadeIn(50);
                    setTimeout(function () { $("#themeSuccess").fadeOut(900); }, 500)
                }
                else {
                    vm.accountSettings.Theme = previousState;
                }
            })
                .error(function (data, status, headers, config) {
                    vm.accountSettings.Theme = previousState;
                })
        }



        /*==========================================
              Custom Domain Settings
         ==========================================*/

        vm.newCustomDomain = null;
        vm.newCustomDomainLoading = false;
        vm.newCustomDomainComplete = false;

        vm.initializeCustomDomainUpdate = function () {
            vm.newCustomDomain = vm.accountSettings.CustomDomain;
            vm.newCustomDomainLoading = false;
            vm.newCustomDomainComplete = false;
        }

        vm.updateCustomDomain = function () {

            vm.newCustomDomainLoading = true;

            accountSettingsServices.updateCustomDomain(vm.newCustomDomain)
            .success(function (data, status, headers, config) {
                vm.newCustomDomainLoading = false;
                if (data.isSuccess) {
                    vm.accountSettings.CustomDomain = vm.newCustomDomain;
                    vm.newCustomDomainComplete = true;
                }
                else {
                    vm.newCustomDomain = vm.accountSettings.CustomDomain;
                }
            })
                .error(function (data, status, headers, config) {
                    vm.newCustomDomain = vm.accountSettings.CustomDomain;
                })
        }

        /*==========================================
              Sales Settings
         ==========================================*/

        vm.updateAccountSettingUseSalesLeads = function (useSalesLeads) {
            var previousState = vm.accountSettings.SalesSettings.UseSalesLeads;
            vm.accountSettings.SalesSettings.UseSalesLeads = null;

            accountSettingsServices.updateUseSalesLeads(useSalesLeads)
            .success(function (data, status, headers, config) {

                if (data.isSuccess) {
                    vm.accountSettings.SalesSettings.UseSalesLeads = useSalesLeads;
                    if (!useSalesLeads)
                    {
                        vm.accountSettings.SalesSettings.UseSalesAlerts = false;
                    }
                }
                else {
                    vm.accountSettings.SalesSettings.UseSalesLeads = previousState;
                }
            })
                .error(function (data, status, headers, config) {
                    vm.accountSettings.SalesSettings.UseSalesLeads = previousState;
                })
        }

        vm.updateAccountSettingUseSalesAlerts = function (useSalesAlerts) {
            var previousState = vm.accountSettings.SalesSettings.UseSalesAlerts;
            vm.accountSettings.SalesSettings.UseSalesAlerts = null;

            accountSettingsServices.updateUseSalesAlerts(useSalesAlerts)
            .success(function (data, status, headers, config) {

                if (data.isSuccess) {
                    vm.accountSettings.SalesSettings.UseSalesAlerts = useSalesAlerts;
                }
                else {
                    vm.accountSettings.SalesSettings.UseSalesAlerts = previousState;
                }
            })
                .error(function (data, status, headers, config) {
                    vm.accountSettings.SalesSettings.UseSalesAlerts = previousState;
                })
        }


        //--- Sales Lead Button Copy ----

        vm.newLeadButtonLabel = null;
        vm.newLeadButtonLabelLoading = false;
        vm.newLeadButtonLabelComplete = false;

        vm.initializeLeadButtonCopyUpdate = function()
        {
            vm.newLeadButtonLabel = vm.accountSettings.SalesSettings.ButtonCopy;
            vm.newLeadButtonLabelLoading = false;
            vm.newLeadButtonLabelComplete = false;
        }

        vm.updateLeadButtonCopy = function () {

            vm.newLeadButtonLabelLoading = true;

            accountSettingsServices.updateSalesLeadsButtonCopy(vm.newLeadButtonLabel)
            .success(function (data, status, headers, config) {
                vm.newLeadButtonLabelLoading = false;
                if (data.isSuccess) {
                    vm.accountSettings.SalesSettings.ButtonCopy = vm.newLeadButtonLabel;
                    vm.newLeadButtonLabelComplete = true;
                }
                else {
                    vm.newLeadButtonLabel = vm.accountSettings.SalesSettings.ButtonCopy;
                }
            })
                .error(function (data, status, headers, config) {
                    vm.newLeadButtonLabel = vm.accountSettings.SalesSettings.ButtonCopy;
                })
        }

        //--- Sales Lead Description Copy ----

        vm.newLeadDescriptionLabel = null;
        vm.newLeadDescriptionLabelLoading = false;
        vm.newLeadDescriptionLabelComplete = false;

        vm.initializeLeadDescriptionCopyUpdate = function () {
            vm.newLeadDescriptionLabel = vm.accountSettings.SalesSettings.DescriptionCopy;
            vm.newLeadDescriptionLabelLoading = false;
            vm.newLeadDescriptionLabelComplete = false;
        }

        vm.updateLeadDescriptionCopy = function () {

            vm.newLeadDescriptionLabelLoading = true;

            accountSettingsServices.updateSalesLeadsDescriptionCopy(vm.newLeadDescriptionLabel)
            .success(function (data, status, headers, config) {
                vm.newLeadDescriptionLabelLoading = false;
                if (data.isSuccess) {
                    vm.accountSettings.SalesSettings.DescriptionCopy = vm.newLeadDescriptionLabel;
                    vm.newLeadDescriptionLabelComplete = true;
                }
                else {
                    vm.newLeadDescriptionLabel = vm.accountSettings.SalesSettings.DescriptionCopy;
                }
            })
                .error(function (data, status, headers, config) {
                    vm.newLeadDescriptionLabel = vm.accountSettings.SalesSettings.DescriptionCopy;
                })
        }


        /*====================================================
                Alert Emails
         ========================================================*/

        vm.alertEmailConstraint = false; //<-- True if reach limit

        vm.newAlertEmail =
             {
                 Email: null,

                 // Service Processing --------

                 IsSending: false,
                 SendingComplete: false,

                 Results: {
                     IsSuccess: false,
                     Message: null
                 },


                 // Cleanup Routine(s) ----------

                 Retry: function () {
                     this.IsSending = false;
                     this.SendingComplete = false;
                 },

                 Clear: function () {

                     this.Email = null;

                     this.IsSending = false;
                     this.SendingComplete = false;

                     this.Results.IsSuccess = false;
                     this.Results.Message = null;
                 }
             }

        vm.addAlertEmail = function() {

            vm.alertEmailConstraint = false;
            vm.newAlertEmail.IsSending = true;

            accountSettingsServices.addSalesAlertEmail(vm.newAlertEmail.Email)
            .success(function (data, status, headers, config) {

                vm.newAlertEmail.IsSending = false;
                vm.newAlertEmail.SendingComplete = true;

                if (data.isSuccess) {

                    vm.newAlertEmail.Results.IsSuccess = true;
                    vm.newAlertEmail.Results.Message = "Email added!";

                    if (vm.accountSettings.SalesSettings.AlertEmails == null)
                    {
                        vm.accountSettings.SalesSettings.AlertEmails = [];
                    }

                    vm.accountSettings.SalesSettings.AlertEmails.push(vm.newAlertEmail.Email);

                    vm.newAlertEmail.Clear();
                    vm.alertEmailConstraint = false;
                }
                else {
                    vm.newAlertEmail.Results.IsSuccess = false;
                    vm.newAlertEmail.Results.Message = data.ErrorMessage;

                    if (data.ErrorCode == "Constraint") {
                        vm.alertEmailConstraint = true;
                    }
                }

            })
                .error(function (data, status, headers, config) {

                    vm.newAlertEmail.Results.IsSuccess = false;

                    //vm.clearInvitationForm();

                    vm.newAlertEmail.IsSending = false;
                    vm.newAlertEmail.SendingComplete = true;
                    vm.newAlertEmail.Results.Message = "An error occurred while attempting to use the service...";
                })
        }


        vm.resetAddAlertEmail = function() {
            vm.newAlertEmail.Clear();
        }


        vm.deletingAlertEmail = false;

        vm.removeAlertEmail = function(index) {

            vm.deletingAlertEmail = true;

            accountSettingsServices.removeSalesAlertEmail(index)
            .success(function (data, status, headers, config) {
                vm.deletingAlertEmail = false;
                if (data.isSuccess) {


                    vm.accountSettings.SalesSettings.AlertEmails.splice(index, 1);


                }
                else {
                    vm.deleteAlertEmailErrorMessage = data.ErrorMessage;
                }

            })
                .error(function (data, status, headers, config) {
                    vm.deletingAlertEmail = false;
                    vm.deleteAlertEmailErrorMessage = "An error occurred while attempting to use the service...";
                })

        }

        vm.deleteAlertEmailErrorMessage = null;

        vm.cleareDeleteAlertEmailErrorMessage = function () {
            vm.deleteAlertEmailErrorMessage = null;
        }


        /*====================================================
                Lead Labels
         ========================================================*/

        //vm.alertEmailConstraint = false; //<-- True if reach limit

        vm.newLeadLabel =
             {
                 Label: null,

                 // Service Processing --------

                 IsSending: false,
                 SendingComplete: false,

                 Results: {
                     IsSuccess: false,
                     Message: null
                 },


                 // Cleanup Routine(s) ----------

                 Retry: function () {
                     this.IsSending = false;
                     this.SendingComplete = false;
                 },

                 Clear: function () {

                     this.Label = null;

                     this.IsSending = false;
                     this.SendingComplete = false;

                     this.Results.IsSuccess = false;
                     this.Results.Message = null;
                 }
             }

        vm.addLeadLabel = function () {

            //vm.alertEmailConstraint = false;
            vm.newLeadLabel.IsSending = true;

            leadsServices.addSalesLeadLabel(vm.newLeadLabel.Label)
            .success(function (data, status, headers, config) {

                vm.newLeadLabel.IsSending = false;
                vm.newLeadLabel.SendingComplete = true;

                if (data.isSuccess) {

                    vm.newLeadLabel.Results.IsSuccess = true;
                    vm.newLeadLabel.Results.Message = "Label added!";

                    if (vm.accountSettings.SalesSettings.LeadLabels == null) {
                        vm.accountSettings.SalesSettings.LeadLabels = [];
                    }

                    vm.accountSettings.SalesSettings.LeadLabels.push(vm.newLeadLabel.Label);

                    vm.newLeadLabel.Clear();
                    //vm.alertEmailConstraint = false;
                }
                else {
                    vm.newLeadLabel.Results.IsSuccess = false;
                    vm.newLeadLabel.Results.Message = data.ErrorMessage;

                    if (data.ErrorCode == "Constraint") {
                        //vm.alertEmailConstraint = true;
                    }
                }

            })
                .error(function (data, status, headers, config) {

                    vm.newLeadLabel.Results.IsSuccess = false;

                    //vm.clearInvitationForm();

                    vm.newLeadLabel.IsSending = false;
                    vm.newLeadLabel.SendingComplete = true;
                    vm.newLeadLabel.Results.Message = "An error occurred while attempting to use the service...";
                })
        }


        vm.resetAddLeadLabel = function () {
            vm.newLeadLabel.Clear();
        }


        vm.deletingLeadLabel = false;

        vm.removeLeadLabel = function (index) {

            vm.deletingLeadLabel = true;

            leadsServices.removeSalesLeadLabel(index)
            .success(function (data, status, headers, config) {
                vm.deletingLeadLabel = false;
                if (data.isSuccess) {


                    vm.accountSettings.SalesSettings.LeadLabels.splice(index, 1);


                }
                else {
                    vm.deleteLeadLabelErrorMessage = data.ErrorMessage;
                }

            })
                .error(function (data, status, headers, config) {
                    vm.deletingLeadLabel = false;
                    vm.deleteLeadLabelErrorMessage = "An error occurred while attempting to use the service...";
                })

        }

        vm.deleteLeadLabelErrorMessage = null;

        vm.cleareDeleteLeadLabelErrorMessage = function () {
            vm.deleteLeadLabelErrorMessage = null;
        }
        
        /*===========================================
            Advanced Sales Settings
         ===========================================

        vm.showAdvancedSalesOptions = false;

        vm.displayAdvancedSalesOptions = function () {
            vm.showAdvancedSalesOptions = true;
        }
        vm.hideAdvancedSalesOptions = function () {
            vm.showAdvancedSalesOptions = false;
        }*/



        /*============================================================
            INVENTORY SETTINGSS SUBMENU
        =======================================================*/

        vm.inventorySettingsSubMenu =
            {
                propertiesButton: true,
                tagsButton: false,
                //imageFormatsButton: false,

                update: function (buttonName) {

                    //Debug.trace(buttonName + " clicked");

                    if (buttonName == 'properties') {
                        this.propertiesButton = true;
                        this.tagsButton = false;
                        //this.imageFormatsButton = false;
                        vm.getProperties();
                    }
                    if (buttonName == 'tags') {
                        this.propertiesButton = false;
                        this.tagsButton = true;
                        //this.imageFormatsButton = false;
                        vm.getTags();
                    }
                    //if (buttonName == 'imageFormats') {
                        //this.propertiesButton = false;
                        //this.tagsButton = false;
                        //this.imageFormatsButton = true;
                        //vm.getImageFormats();
                    //}
                },
            }





        /* ==========================================
            TAG METHODS
        ==========================================*/

        vm.tagConstraint = false; //<-- True if plan needs an upgrade

        vm.newTag =
             {
                 Name: null,

                 // Service Processing --------

                 IsSending: false,
                 SendingComplete: false,

                 Results: {
                     IsSuccess: false,
                     Message: null
                 },


                 // Cleanup Routine(s) ----------

                 Retry: function () {
                     this.IsSending = false;
                     this.SendingComplete = false;
                 },

                 Clear: function () {
                     //Debug.trace("Clearing new tag form data.");

                     this.Name = null;

                     this.IsSending = false;
                     this.SendingComplete = false;

                     this.Results.IsSuccess = false;
                     this.Results.Message = null;
                 }
             }

        vm.createTag = function () {

            vm.tagConstraint = false;
            vm.newTag.IsSending = true;

            //Debug.trace("Creating tag...");

            tagServices.createTag(vm.newTag.Name, vm.newTag.Visible)
            .success(function (data, status, headers, config) {

                vm.newTag.IsSending = false;
                vm.newTag.SendingComplete = true;

                if (data.isSuccess) {

                    vm.newTag.Results.IsSuccess = true;
                    vm.newTag.Results.Message = "Tag created!";
                    vm.getTags();
                }
                else {
                    vm.newTag.Results.IsSuccess = false;
                    vm.newTag.Results.Message = data.ErrorMessage;

                    if (data.ErrorCode == "Constraint") {
                        vm.tagConstraint = true;
                    }
                }

            })
                .error(function (data, status, headers, config) {

                    vm.newTag.Results.IsSuccess = false;

                    //vm.clearInvitationForm();

                    vm.newTag.IsSending = false;
                    vm.newTag.SendingComplete = true;
                    vm.newTag.Results.Message = "An error occurred while attempting to use the service...";
                })
        }

        vm.tags = null;

        vm.getTags = function () {

            //Debug.trace("Getting tags...");

            tagServices.getTags()
            .success(function (data, status, headers, config) {

                vm.tags = data;

            })
            .error(function (data, status, headers, config) {

            })
        }


        vm.tagDetails =
         {
             TagName: null,

             // Service Processing --------

             IsSending: false,
             SendingComplete: false,

             Results: {
                 IsSuccess: false,
                 Message: null
             },

             Setup: function (index) {

                 this.TagName = vm.tags[index];

                 this.IsSending = false;
                 this.SendingComplete = false;

                 this.Results.IsSuccess = false;
                 this.Results.Message = null;
             },

         }

        vm.deleteTag = function () {

            vm.tagDetails.IsSending = true;

            //Debug.trace("Deleting tag...");

            tagServices.deleteTag(vm.tagDetails.TagName)
            .success(function (data, status, headers, config) {

                vm.tagDetails.IsSending = false;
                vm.tagDetails.SendingComplete = true;

                if (data.isSuccess) {

                    vm.tagDetails.Results.IsSuccess = true;
                    vm.tagDetails.Results.Message = "Tag deleted!";
                    vm.getTags();
                }
                else {
                    vm.tagDetails.Results.IsSuccess = false;
                    vm.tagDetails.Results.Message = data.ErrorMessage;
                }

            })
            .error(function (data, status, headers, config) {
                vm.tagDetails.Results.IsSuccess = false;

                vm.tagDetails.IsSending = false;
                vm.tagDetails.SendingComplete = true;
                vm.tagDetails.Results.Message = "An error occurred while attempting to use the service...";
            })
        }

        /* ==========================================
            END TAG METHODS
        ==========================================*/



        /* ==========================================

            PROPERTY METHODS

        ==========================================*/

        /* ==========================================
           MANAGE PROPERTY
        ==========================================*/
        vm.featuredPropertiesUnordered = [],
        vm.featuredProperties = [],
        vm.featuredPropertiesDiff = [],

        vm.currentPropertiesIndex = 0; //<-- If in details we need to repoplulate values on upate

        vm.propertyTypes = null;

        vm.getPropertyTypes = function () {

            if (vm.propertyTypes == null) //<-- We only want to get this once as it never changes
            {
                //Debug.trace("Getting property types...");

                propertiesServices.getPropertyTypes()
                .success(function (data, status, headers, config) {

                    vm.propertyTypes = data;
                })
                .error(function (data, status, headers, config) {

                })
            }

        }

        vm.getProperties = function () {

            vm.initiateSwatchImageUploader();

            //Debug.trace("Getting properties...");

            propertiesServices.getProperties()
            .success(function (data, status, headers, config) {

                //Reset featured:
                vm.featuredPropertiesUnordered = [],
                vm.featuredProperties = [],
                vm.featuredPropertiesDiff = [],

                vm.properties = data;
                
                vm.propertyDetail = vm.properties[vm.currentPropertiesIndex];

                //Get all featured properties
                for (var i = 0; i < vm.properties.length; i++) {
                    if(vm.properties[i].FeaturedID > 0)
                    {
                        vm.featuredPropertiesUnordered.push(vm.properties[i]);
        
                    }
                    else
                    {
                        vm.featuredPropertiesDiff.push(vm.properties[i]);
                    }
                }

                //Reorder by FeatureID
                for (var i = 1; i < (vm.featuredPropertiesUnordered.length+1); i++) {
                    for (var x = 0; x < vm.featuredPropertiesUnordered.length; x++) {

                        if (vm.featuredPropertiesUnordered[x].FeaturedID == i) {
                            vm.featuredProperties.push(vm.featuredPropertiesUnordered[x]);
                        }
                    }
                }

                
            })
            .error(function (data, status, headers, config) {

            })
        }

        vm.getPropertyDetail = function (index, reset) {

            vm.currentPropertiesIndex = index;

            vm.propertyDetail = vm.properties[index];
            vm.propertyDetailReset = true;

            vm.newPropertyValue.Clear();
            vm.propertyValueConstraint = false;
        }

        // --- CREATE PROPERTY

        vm.propertyConstraint = false; //<-- True if plan needs an upgrade

        vm.newProperty =
             {
                 Locked: false,
                 Name: null,
                 PropertyType: null,

                 // Service Processing --------

                 IsSending: false,
                 SendingComplete: false,

                 Results: {
                     IsSuccess: false,
                     Message: null
                 },

                 // Visibiliy ----

                 Hide: function () {
                     this.Locked = false;
                 },

                 Show: function () {
                     this.Locked = true;
                 },

                 // Cleanup Routine(s) ----------

                 Retry: function () {
                     this.IsSending = false;
                     this.SendingComplete = false;
                 },

                 Clear: function () {

                     //Debug.trace("Clearing new property form data.");

                     vm.getPropertyTypes();

                     this.Name = null;
                     this.Locked = false;
                     this.PropertyType = null;

                     this.IsSending = false;
                     this.SendingComplete = false;

                     this.Results.IsSuccess = false;
                     this.Results.Message = null;
                 }
             }


        vm.setNewPropertyType = function(index)
        {
            vm.newProperty.PropertyType = vm.propertyTypes[index];
        }

        vm.initializeNewPropertyCreate = function()
        {
            vm.getPropertyTypes();
            vm.newProperty.Clear();
        }

        vm.createProperty = function () {

            vm.propertyConstraint = false;
            vm.newProperty.IsSending = true;

            //Debug.trace("Creating property...");

            propertiesServices.createProperty(vm.newProperty.PropertyType.PropertyTypeNameKey, vm.newProperty.Name)
            .success(function (data, status, headers, config) {

                vm.newProperty.IsSending = false;
                vm.newProperty.SendingComplete = true;

                if (data.isSuccess) {

                    vm.newProperty.Results.IsSuccess = true;
                    vm.newProperty.Results.Message = "Property created!";
                    vm.getProperties();
                }
                else {
                    vm.newProperty.Results.IsSuccess = false;
                    vm.newProperty.Results.Message = data.ErrorMessage;

                    if (data.ErrorCode == "Constraint") {
                        vm.propertyConstraint = true;
                    }
                }

            })
                .error(function (data, status, headers, config) {

                    vm.newProperty.Results.IsSuccess = false;

                    //vm.clearInvitationForm();

                    vm.newProperty.IsSending = false;
                    vm.newProperty.SendingComplete = true;
                    vm.newProperty.Results.Message = "An error occurred while attempting to use the service...";
                })
        }


        /* MANAGE PROPERTY LISTINGS STATE */

        
        vm.addPropertyToListings = function (index) {

            vm.properties[index].Listing = null;

            propertiesServices.updatePropertyListingState(vm.properties[index].PropertyNameKey, true)
            .success(function (data, status, headers, config) {

                if (data.isSuccess) {
                    vm.properties[index].Listing = true;
                }
                else {
                    vm.properties[index].Listing = false;
                }
            })
                .error(function (data, status, headers, config) {
                    vm.properties[index].Listing = false;
                })
        }

        vm.hidePropertyFromListings = function (index) {

            vm.properties[index].Listing = null;

            propertiesServices.updatePropertyListingState(vm.properties[index].PropertyNameKey, false)
            .success(function (data, status, headers, config) {

                if (data.isSuccess) {
                    vm.properties[index].Listing = false;
                }
                else {
                    vm.properties[index].Listing = true;
                }
            })
                .error(function (data, status, headers, config) {
                    vm.properties[index].Listing = true;
                })
        }


        /* MANAGE PROPERTY DETAILS STATE */


        vm.addPropertyToDetails = function (index) {

            vm.properties[index].Details = null;

            propertiesServices.updatePropertyDetailsState(vm.properties[index].PropertyNameKey, true)
            .success(function (data, status, headers, config) {

                if (data.isSuccess) {
                    vm.properties[index].Details = true;
                }
                else {
                    vm.properties[index].Details = false;
                }
            })
                .error(function (data, status, headers, config) {
                    vm.properties[index].Details = false;
                })
        }

        vm.hidePropertyFromDetails = function (index) {

            vm.properties[index].Details = null;

            propertiesServices.updatePropertyDetailsState(vm.properties[index].PropertyNameKey, false)
            .success(function (data, status, headers, config) {

                if (data.isSuccess) {
                    vm.properties[index].Details = false;
                }
                else {
                    vm.properties[index].Details = true;
                }
            })
                .error(function (data, status, headers, config) {
                    vm.properties[index].Details = true;
                })
        }

        
        /* ==========================================
           END MANAGE PROPERTY
        ==========================================*/


        /* ==========================================
          MANAGE FEATURED PROPERTIES
       ==========================================*/

        vm.movingFeaturedProperty = false;

        vm.removePropertyFeaturedCollectionItem = function (index) {

            vm.movingFeaturedProperty = true;
            vm.featuredPropertiesIsSending = false;
            vm.featuredPropertiesSendingComplete = false;
            vm.featuredPropertiesResultsIsSuccess = false;
            vm.featuredPropertiesResultsMessage = null;

            if (vm.featuredProperties == null) {
                vm.featuredProperties = [];
            }
            var placeholderArray = [];

            //Get all featured property IDs
            for (var i = 0; i < vm.featuredProperties.length; i++) {
                placeholderArray.push(vm.featuredProperties[i].PropertyID);
            }

            placeholderArray.splice(index, 1);

            if (placeholderArray.length > 0)
            {
                propertiesServices.updateFeaturedProperties(placeholderArray)
                .success(function (data, status, headers, config) {

                    vm.movingFeaturedProperty = false;

                    if (data.isSuccess) {

                        vm.featuredPropertiesDiff.push(vm.featuredProperties[index]);
                        //vm.productProperties[vm.featuredPropertiesCurrentIndex].AssignedValues.splice(index, 1); //<-- (Not needed) Holds a reference to the array (below) so they are in sync.
                        vm.featuredProperties.splice(index, 1);  //<-- Updates the array above as well since they are already in sync.

                    }
                    else {
                        vm.featuredPropertiesIsSending = false;
                        vm.featuredPropertiesSendingComplete = true;
                        vm.featuredPropertiesResultsIsSuccess = false;
                        vm.featuredPropertiesResultsMessage = data.ErrorMessage;
                    }
                })
                .error(function (data, status, headers, config) {
                    vm.movingFeaturedProperty = false;
                    vm.featuredPropertiesIsSending = false;
                    vm.featuredPropertiesSendingComplete = true;
                    vm.featuredPropertiesResultsIsSuccess = false;
                    vm.featuredPropertiesResultsMessage = "Could not send update to server.";
                })
            }
            else {
                propertiesServices.resetFeaturedProperties()
                .success(function (data, status, headers, config) {

                    vm.movingFeaturedProperty = false;

                    if (data.isSuccess) {

                        vm.featuredPropertiesDiff.push(vm.featuredProperties[index]);
                        //vm.productProperties[vm.featuredPropertiesCurrentIndex].AssignedValues.splice(index, 1); //<-- (Not needed) Holds a reference to the array (below) so they are in sync.
                        vm.featuredProperties.splice(index, 1);  //<-- Updates the array above as well since they are already in sync.

                    }
                    else {
                        vm.featuredPropertiesIsSending = false;
                        vm.featuredPropertiesSendingComplete = true;
                        vm.featuredPropertiesResultsIsSuccess = false;
                        vm.featuredPropertiesResultsMessage = data.ErrorMessage;
                    }
                })
                .error(function (data, status, headers, config) {
                    vm.movingFeaturedProperty = false;
                    vm.featuredPropertiesIsSending = false;
                    vm.featuredPropertiesSendingComplete = true;
                    vm.featuredPropertiesResultsIsSuccess = false;
                    vm.featuredPropertiesResultsMessage = "Could not send update to server.";
                })
            }
        }

        vm.addPropertyFeaturedCollectionItem = function (index) {

            vm.movingFeaturedProperty = true;
            vm.featuredPropertiesIsSending = false;
            vm.featuredPropertiesSendingComplete = false;
            vm.featuredPropertiesResultsIsSuccess = false;
            vm.featuredPropertiesResultsMessage = null;

            var valueToAdd = vm.featuredPropertiesDiff[index];
            
            if (vm.featuredProperties == null) {
                vm.featuredProperties = [];
            }
            var placeholderArray = [];

            //Get all featured property IDs
            for (var i = 0; i < vm.featuredProperties.length; i++) {
                placeholderArray.push(vm.featuredProperties[i].PropertyID);
            }

            placeholderArray.push(valueToAdd.PropertyID);

            //Debug.trace(placeholderArray);

            propertiesServices.updateFeaturedProperties(placeholderArray)
            .success(function (data, status, headers, config) {
                vm.movingFeaturedProperty = false;
                if (data.isSuccess) {

                    
                    vm.featuredPropertiesDiff.splice(index, 1);

                    if (vm.featuredProperties == null) {
                        vm.featuredProperties = [];
                    }
                    vm.featuredProperties.push(valueToAdd);

                    //if (vm.productProperties[vm.featuredPropertiesCurrentIndex].AssignedValues == null) {
                        //vm.productProperties[vm.featuredPropertiesCurrentIndex].AssignedValues = []; //<-- (Not needed) Will now hold a reference to the other array so they are in sync
                        //vm.productProperties[vm.featuredPropertiesCurrentIndex].AssignedValues = vm.featuredPropertiesAssignedValues; //<-- Will now hold a reference to the other array so they are in sync
                    //}
                    //vm.productProperties[vm.featuredPropertiesCurrentIndex].AssignedValues.push(valueToAdd); //<-- (Not needed) Will now hold a reference to the other array so they are in sync
                }
                else {
                    vm.featuredPropertiesIsSending = false;
                    vm.featuredPropertiesSendingComplete = true;
                    vm.featuredPropertiesResultsIsSuccess = false;
                    vm.featuredPropertiesResultsMessage = data.ErrorMessage;
                }
            })
            .error(function (data, status, headers, config) {

                vm.movingFeaturedProperty = false;
                vm.featuredPropertiesIsSending = false;
                vm.featuredPropertiesSendingComplete = true;
                vm.featuredPropertiesResultsIsSuccess = false;
                vm.featuredPropertiesResultsMessage = "Could not send update to server.";
            })
        }





        /* ==========================================
           MANAGE PROPERTY VALUES
        ==========================================*/

        // --- CREATE PROPERTY VALUE

        vm.propertyValueConstraint = false; //<-- True if plan needs an upgrade

        vm.newPropertyValue =
             {
                 Name: null,

                 // Service Processing --------

                 IsSending: false,
                 SendingComplete: false,

                 Results: {
                     IsSuccess: false,
                     Message: null
                 },


                 // Cleanup Routine(s) ----------

                 Retry: function () {
                     this.IsSending = false;
                     this.SendingComplete = false;
                 },

                 Clear: function () {
                     //Debug.trace("Clearing new value form data.");

                     this.Name = null;

                     this.IsSending = false;
                     this.SendingComplete = false;

                     this.Results.IsSuccess = false;
                     this.Results.Message = null;
                 }
             }

        vm.createPropertyValue = function () {

            vm.propertyValueConstraint = false;
            vm.newPropertyValue.IsSending = true;

            //Debug.trace("Creating value...");

            propertiesServices.createPropertyValue(vm.propertyDetail.PropertyNameKey, vm.newPropertyValue.Name)
            .success(function (data, status, headers, config) {

                vm.newPropertyValue.IsSending = false;
                vm.newPropertyValue.SendingComplete = true;

                if (data.isSuccess) {

                    vm.newPropertyValue.Results.IsSuccess = true;
                    vm.newPropertyValue.Results.Message = "Value created!";

                    vm.getProperties();

                    vm.newPropertyValue.Clear();
                    vm.propertyValueConstraint = false;
                }
                else {
                    vm.newPropertyValue.Results.IsSuccess = false;
                    vm.newPropertyValue.Results.Message = data.ErrorMessage;

                    if (data.ErrorCode == "Constraint") {
                        vm.propertyValueConstraint = true;
                    }
                }

            })
                .error(function (data, status, headers, config) {

                    vm.newPropertyValue.Results.IsSuccess = false;

                    //vm.clearInvitationForm();

                    vm.newPropertyValue.IsSending = false;
                    vm.newPropertyValue.SendingComplete = true;
                    vm.newPropertyValue.Results.Message = "An error occurred while attempting to use the service...";
                })
        }


        vm.resetCreatePropertyValue = function () {
            vm.newPropertyValue.Clear();
        }


        vm.deletingPropertyValue = false;

        vm.deletePropertyValue = function (index) {
            
            vm.deletingPropertyValue = true;

            propertiesServices.deletePropertyValue(vm.propertyDetail.PropertyNameKey, vm.propertyDetail.Values[index].PropertyValueNameKey)
            .success(function (data, status, headers, config) {
                vm.deletingPropertyValue = false;
                if (data.isSuccess) {
                    vm.getProperties();
                }
                else {
                    vm.deletePropertyValueErrorMessage = data.ErrorMessage;
                }

            })
                .error(function (data, status, headers, config) {
                    vm.deletingPropertyValue = false;
                    vm.deletePropertyValueErrorMessage = "An error occurred while attempting to use the service...";
                })

        }

        vm.deletePropertyValueErrorMessage = null;

        vm.cleareDeletePropertyValueErrorMessage = function()
        {
            vm.deletePropertyValueErrorMessage = null;
        }


        /* ==========================================
           END MANAGE PROPERTY VALUES
        ==========================================*/




        /* ==========================================
           MANAGE SWATCH VALUES
        ==========================================*/

        // --- CREATE SWATCH VALUE

        vm.swatchValueConstraint = false; //<-- True if plan needs an upgrade

        vm.newSwatchValue =
               {
                 Label: null,
                 Url: null,

                 // Service Processing --------

                 IsSending: false,
                 SendingComplete: false,

                 Results: {
                     IsSuccess: false,
                     Message: null
                 },


                 // Cleanup Routine(s) ----------

                 Retry: function () {
                     this.IsSending = false;
                     this.SendingComplete = false;
                 },

                 Clear: function () {
                     //Debug.trace("Clearing new value form data.");

                     this.Label = null;
                     this.Url = null;

                     this.IsSending = false;
                     this.SendingComplete = false;

                     this.Results.IsSuccess = false;
                     this.Results.Message = null;
                 }
             }

        vm.createSwatchValue = function () {

            vm.swatchValueConstraint = false;
            vm.newSwatchValue.IsSending = true;

            //Debug.trace("Creating swatch...");

            propertiesServices.createSwatchValue(vm.propertyDetail.PropertyNameKey, vm.newSwatchValue.Url, vm.newSwatchValue.Label)
            .success(function (data, status, headers, config) {


             
                vm.newSwatchValue.IsSending = false;
                vm.newSwatchValue.SendingComplete = true;

                if (data.isSuccess) {

                    $("#swatchImage").css('background-image', 'url(/Images/ui/fpo/swatch-empty-slot2.jpg)');
                    $("#swatchLabel").css('color', 'Darkgray');
                    $("#swatchInput").prop("disabled", true);

                    vm.newSwatchValue.Results.IsSuccess = true;
                    vm.newSwatchValue.Results.Message = "Value created!";

                    vm.getProperties();

                    vm.newSwatchValue.Clear();
                    vm.swatchValueConstraint = false;
                }
                else {
                    vm.newSwatchValue.Results.IsSuccess = false;
                    vm.newSwatchValue.Results.Message = data.ErrorMessage;

                    if (data.ErrorCode == "Constraint") {
                        vm.swatchValueConstraint = true;
                    }
                }

            })
                .error(function (data, status, headers, config) {

                    vm.newSwatchValue.Results.IsSuccess = false;

                    //vm.clearInvitationForm();

                    vm.newSwatchValue.IsSending = false;
                    vm.newSwatchValue.SendingComplete = true;
                    vm.newSwatchValue.Results.Message = "An error occurred while attempting to use the service...";
                })
        }


        vm.resetCreateSwatchValue = function () {
            vm.newSwatchValue.Clear();
        }


        vm.deletingSwatchValue = false;

        vm.deleteSwatchValue = function (index) {

            vm.deletingSwatchValue = true;

            propertiesServices.deleteSwatchValue(vm.propertyDetail.PropertyNameKey, vm.propertyDetail.Swatches[index].PropertySwatchNameKey)
            .success(function (data, status, headers, config) {
                vm.deletingSwatchValue = false;
                if (data.isSuccess) {
                    vm.getProperties();
                }
                else {
                    vm.deleteSwatchValueErrorMessage = data.ErrorMessage;
                }

            })
                .error(function (data, status, headers, config) {
                    vm.deletingSwatchValue = false;
                    vm.deleteSwatchValueErrorMessage = "An error occurred while attempting to use the service...";
                })

        }

        vm.deleteSwatchValueErrorMessage = null;

        vm.cleareDeleteSwatchValueErrorMessage = function () {
            vm.deleteSwatchValueErrorMessage = null;
        }


        /* ==========================================
           Check for Swatch / Update Swatch Image
       ==========================================*/

        vm.initiateSwatchImageUploader = function()
        {
            // Add events to input to fire when status changes
            document.getElementById("swatchUploadInput").addEventListener('change', uploadSwatchImg);

            //Add function to photo div to trigger overlay state on hover
            $('#swatchImage').hover(function () {
                $("#swatchImageOverlay").fadeIn(120);
            }, function () {
                $("#swatchImageOverlay").fadeOut(160);
            })
        }

        //For testing only
        //vm.newSwatchValue.Url = "https://c1.staticflickr.com/7/6112/6255024094_ef871657c4_b.jpg";


        // Grab the files and set them to our variable
        function uploadSwatchImg() {

            //Fade out/clear all hover & error messages
            $("#swatchImageOverlay").fadeOut(150);
            $("#swatchUploadErrorText").text("");
            $("#swatchUploadErrorMessage").slideUp();

            // Check for the various HTML5 File API support.
            //if (window.File && window.FileReader && window.FileList && window.Blob) {

            // Great! All the File APIs are supported.

            //Debug.trace("vm.uploadingSwatch = " + vm.uploadingSwatch);

            var file = document.getElementById('swatchUploadInput').files[0];

            //Debug.trace("size=" + file.size);
            //Debug.trace("type=" + file.type);
            //Debug.trace("result=" + file.result);

            if (file.type != "image/jpeg" && file.type != "image/png" && file.type != "image/gif" && file.type != "image/bmp") { // <-- Tiff features in future
                //Debug.trace("Not a supported image format");
                //vm.uploadingSwatch = false;
                $("#uploadingSwatchAnimation").fadeOut(100);
                $("#swatchUploadErrorText").text("Please only upload an image of type Jpeg, Png, Gif or Bmp.");
                $("#swatchUploadErrorMessage").slideDown('slow').delay(1900).slideUp('slow');
            }
            else if (file.size > "5000000") // (in Bytes) (5mb) 
            {
                // *!!* (WCF Settings) MaxSize Must also be updated in Sahara.CoreServices.Host.CreateServiceHost()
                // *!!* (Local Settings) MaxSize Must also be updated in local Web.Config > Configuration > System.Web > httpRuntime > maxRequestLength="10000" (in KB)                    
                //Debug.trace("File too large");
                $("#uploadingSwatchAnimation").fadeOut(100);
                $("#swatchUploadErrorText").text("Image size too large. Max allowed is 5mb.");
                $("#swatchUploadErrorMessage").slideDown('slow').delay(1900).slideUp('slow');
            }
            else {

                //Show Loader
                $("#uploadingSwatchAnimation").fadeIn(200);

                propertiesServices.uploadSwatchImage(file).onreadystatechange = function () {
                    if (this.readyState == 4) { //<--0 = notinitialized, 1 = set up, 2 = sent, 3 = in process, 4 = complete

                        var data = JSON.parse(this.responseText);
                        //Debug.trace(data);
                        //vm.uploadingSwatch = false;

                        vm.newSwatchValue.Url = data;
                        //Debug.trace("vm.newSwatchValue.Url = " + vm.newSwatchValue.Url);

                        $("#uploadingSwatchAnimation").fadeOut(350);

                        $("#swatchImage").css('background-image', 'url(' + vm.newSwatchValue.Url + ')');
                        $("#swatchLabel").css('color', 'Black');
                        $("#swatchInput").prop( "disabled", false );
                        

                        //if (data.isSuccess) {
                            //vm.newSwatchValue.Url = data.SuccessMessage;
                            //Debug.trace("SwatchImage = " + vm.newSwatchValue.Url);

                            //refresh currentUserProfile object as well as local site cookies...
                            //updateCurrentUserProfile();
                        //}
                        //else {
                            //Show Error
                            //vm.photoUploadFail = true;
                            //vm.photoUploadFailMessage = data.ErrorMessage;
                            //$("#swatchUploadErrorText").text(data.ErrorMessage);
                            //$("#swatchUploadErrorMessage").slideDown('slow').delay(1900).slideUp('slow');
                        //}

                    }
                }

            }
        }

        //*Helper Methods -----------------------------------------------------------------------------------

        //Add function to photo div to trigger hidden file input when clicked
        vm.initiateSwatchUpdate = function () {

            //Click's upload button on hidden file input
            $("#swatchUploadInput:hidden").trigger('click');

        }



        //Check for photo function. Used by GetCurrentProfile() & uploadSwatch, uses default photo if null
/*
        vm.checkPhoto = function () {
            //Debug.trace("Checking photo. vm.currentUserProfile.Photo = " + vm.currentUserProfile.Photo);
            if (vm.currentUserProfile.Photo == "" || vm.currentUserProfile.Photo == null) {
                vm.currentUserProfile.Photo = 'images/icons/profile/nophoto'
            }
            else {
                vm.currentUserProfile.Photo = vm.cdnUri + "/" + vm.currentUserProfile.AccountID + "/userphotos/" + vm.currentUserProfile.Photo;
            }
        }*/

        /* ==========================================
            End Swatch Image Upload
         ==========================================*/

        /* ==========================================
           END MANAGE SWATCH VALUES
        ==========================================*/









        /* =========================================
            SORTABLE STATE
        ========================================*/

        vm.makeSortable = function () {
            vm.propertyDetail.Sortable = null;

            propertiesServices.updateSortableState(vm.propertyDetail.PropertyNameKey, true)
            .success(function (data, status, headers, config) {

                if (data.isSuccess) {
                    vm.propertyDetail.Sortable = true;
                }
                else {
                    vm.propertyDetail.Sortable = false;
                }
            })
                .error(function (data, status, headers, config) {
                    vm.propertyDetail.Sortable = false;
                })
        }

        vm.makeNotSortable = function () {
            vm.propertyDetail.Sortable = null;

            propertiesServices.updateSortableState(vm.propertyDetail.PropertyNameKey, false)
            .success(function (data, status, headers, config) {

                if (data.isSuccess) {
                    vm.propertyDetail.Sortable = false;
                }
                else {
                    vm.propertyDetail.Sortable = true;
                }
            })
                .error(function (data, status, headers, config) {
                    vm.propertyDetail.Sortable = true;
                })
        }


        /* =========================================
            APPENDABLE STATE
        ========================================*/

        vm.makeAppendable = function () {
            vm.propertyDetail.Appendable = null;

            propertiesServices.updateAppendableState(vm.propertyDetail.PropertyNameKey, true)
            .success(function (data, status, headers, config) {

                if (data.isSuccess) {
                    vm.propertyDetail.Appendable = true;
                }
                else {
                    vm.propertyDetail.Appendable = false;
                }
            })
                .error(function (data, status, headers, config) {
                    vm.propertyDetail.Appendable = false;
                })
        }

        vm.makeNotAppendable = function () {
            vm.propertyDetail.Appendable = null;

            propertiesServices.updateAppendableState(vm.propertyDetail.PropertyNameKey, false)
            .success(function (data, status, headers, config) {

                if (data.isSuccess) {
                    vm.propertyDetail.Appendable = false;
                }
                else {
                    vm.propertyDetail.Appendable = true;
                    vm.showPropertyDetailsErrorMessage = true;
                    vm.propertyDetailsErrorMessage = data.ErrorMessage;
                }
            })
                .error(function (data, status, headers, config) {
                    vm.propertyDetail.Appendable = true;

                    vm.showPropertyDetailsErrorMessage = true;
                    vm.propertyDetailsErrorMessage = "An unknown error occured";
                })
        }


        /* =========================================
            FACETABLE STATE
        ========================================*/

        vm.makeFacetable = function () {
            vm.propertyDetail.Facetable = null;

            propertiesServices.updateFacetableState(vm.propertyDetail.PropertyNameKey, true)
            .success(function (data, status, headers, config) {

                if (data.isSuccess) {
                    vm.propertyDetail.Facetable = true;
                }
                else {
                    vm.propertyDetail.Facetable = false;
                }
            })
                .error(function (data, status, headers, config) {
                    vm.propertyDetail.Facetable = false;
                })
        }

        vm.makeNotFacetable = function () {
            vm.propertyDetail.Facetable = null;

            propertiesServices.updateFacetableState(vm.propertyDetail.PropertyNameKey, false)
            .success(function (data, status, headers, config) {

                if (data.isSuccess) {
                    vm.propertyDetail.Facetable = false;
                }
                else {
                    vm.propertyDetail.Facetable = true;
                }
            })
                .error(function (data, status, headers, config) {
                    vm.propertyDetail.Facetable = true;
                })
        }


        /* ==========================================
           FACET INTERVAL
        ==========================================*/

        vm.FacetIntervalStatus =
            {
                Updating: false,
                Editing: false,
                SendingComplete: false,

                NewFacetInterval: null,

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

                    //Debug.trace("Clearing intervale form data.");
                    this.Updating = false;
                    this.Editing = false;
                    this.SendingComplete = false;
                }


            }

        vm.editFacetInterval = function () {
            //Debug.trace("Editing facet interval...");

            vm.FacetIntervalStatus.NewFacetInterval = vm.propertyDetail.FacetInterval;

            vm.FacetIntervalStatus.Editing = true;

        }

        vm.cancelUpdateFacetInterval = function () {

            vm.FacetIntervalStatus.Clear();

        }

        vm.updateFacetInterval = function () {
            //Debug.trace("Updating facet interval...");

            vm.FacetIntervalStatus.Updating = true;
            vm.FacetIntervalStatus.Editing = false;

            propertiesServices.updateFacetInterval(vm.propertyDetail.PropertyNameKey, vm.FacetIntervalStatus.NewFacetInterval)
                .success(function (data, status, headers, config) {

               vm.FacetIntervalStatus.Updating = false;
               vm.FacetIntervalStatus.SendingComplete = true;

               if (data.isSuccess) {

                   vm.FacetIntervalStatus.Results.IsSuccess = true;
                   vm.FacetIntervalStatus.Results.Message = "Facet interval has been updated.";

                   //Update FacetInterval
                   vm.propertyDetail.FacetInterval = vm.FacetIntervalStatus.NewFacetInterval;

                   vm.FacetIntervalStatus.Clear();

               }
               else {
                   vm.FacetIntervalStatus.Results.IsSuccess = false;
                   vm.FacetIntervalStatus.Results.Message = data.ErrorMessage;

               }

           }).error(function (data, status, headers, config) {

               vm.FacetIntervalStatus.IsSuccess = false;
               vm.FacetIntervalStatus.Results.Message = "An error has occurred while using the service.";
           })

        }

        vm.cancelFacetIntervalEdit = function () {

            vm.FacetIntervalStatus.Editing = false;
            vm.FacetIntervalStatus.Updating = false;
        }

        /* ==========================================
           END FACET INTERVAL
        ==========================================*/




        /* ==========================================
            SYMBOL
        ==========================================*/

        vm.SymbolStatus =
            {
                Updating: false,
                Editing: false,
                SendingComplete: false,

                NewSymbol: null,

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

                    //Debug.trace("Clearing form data.");
                    this.Updating = false;
                    this.Editing = false;
                    this.SendingComplete = false;
                }


            }

        vm.editSymbol = function () {
            //Debug.trace("Editing symbol...");

            vm.SymbolStatus.NewSymbol = vm.propertyDetail.Symbol;

            vm.SymbolStatus.Editing = true;

        }

        vm.cancelUpdateSymbol = function () {

            vm.SymbolStatus.Clear();

        }

        vm.updateSymbol = function () {
            //Debug.trace("Updating symbol...");

            vm.SymbolStatus.Updating = true;
            vm.SymbolStatus.Editing = false;


            propertiesServices.updateSymbol(vm.propertyDetail.PropertyNameKey, vm.SymbolStatus.NewSymbol)
                .success(function (data, status, headers, config) {

                    vm.SymbolStatus.Updating = false;
                    vm.SymbolStatus.SendingComplete = true;

                    if (data.isSuccess) {

                        vm.SymbolStatus.Results.IsSuccess = true;
                        vm.SymbolStatus.Results.Message = "symbol has been updated.";

                        //Update Symbol
                        vm.propertyDetail.Symbol = vm.SymbolStatus.NewSymbol;

                        vm.SymbolStatus.Clear();

                    }
                    else {
                        vm.SymbolStatus.Results.IsSuccess = false;
                        vm.SymbolStatus.Results.Message = data.ErrorMessage;

                    }

                }).error(function (data, status, headers, config) {

                    vm.SymbolStatus.IsSuccess = false;
                    vm.SymbolStatus.Results.Message = "An error has occurred while using the service.";
                })

        }

        vm.cancelSymbolEdit = function () {

            vm.SymbolStatus.Editing = false;
            vm.SymbolStatus.Updating = false;
        }

        /* ==========================================
           END SYMBOL
        ==========================================*/

        /* ==========================================
           SYMBOL PLACEMENT
        ==========================================*/

        vm.updateSymbolPlacement = function (placement) {
            //Debug.trace("Updating symbol placement...");

            propertiesServices.updateSymbolPlacement(vm.propertyDetail.PropertyNameKey, placement)
                .success(function (data, status, headers, config) {

                    if (data.isSuccess) {

                        vm.propertyDetail.SymbolPlacement = placement;

                    }
                    else {

                    }

                }).error(function (data, status, headers, config) {

                })

        }

        /* ==========================================
           END SYMBOL PLACEMENT
        ==========================================*/

        /* ==========================================

            END PROPERTY METHODS

        ==========================================*/
















        /* ==========================================

            IMAGE FORMATS METHODS

        ==========================================*/

        vm.imageGroupTypes = null;


        vm.loadImageFormatsPanel = function () {

            //Debug.trace("Getting image group types..");
            
            imageFormatServices.getImageGroupTypes()
            .success(function (data, status, headers, config) {

                vm.imageGroupTypes = data               

            })
            .error(function (data, status, headers, config) {

            })
        }



        /*============================================
         * Format Group CREATE
         ============================================*/


        vm.newImageGroup =
             {

                 ImageGroupTypeName: null,
                 ImageGroupTypeNameKey: null,
                 ImageGroupName: null,

                 // Service Processing --------

                 IsSending: false,
                 SendingComplete: false,

                 Results: {
                     IsSuccess: false,
                     Message: null
                 },

                 UpdateGroupType: function (groupType, groupTypeNameKey) {
                     this.ImageGroupTypeName = groupType;
                     this.ImageGroupTypeNameKey = groupTypeNameKey;
                 },

                 // Cleanup Routine(s) ----------

                 Retry: function () {
                     this.IsSending = false;
                     this.SendingComplete = false;
                 },

                 Clear: function () {

                     //Debug.trace("Clearing new format group form data.");

                     this.ImageGroupTypeName = null;
                     this.ImageGroupTypeNameKey = null;
                     this.ImageGroupName = null;

                     this.IsSending = false;
                     this.SendingComplete = false;

                     this.Results.IsSuccess = false;
                     this.Results.Message = null;
                 }
             }


        vm.initializeCreateImageGroup = function () {
            vm.newImageGroup.Clear();
        }

        vm.createImageGroup = function () {

            vm.newImageGroup.IsSending = true;

            //Debug.trace("Creating image group...");

            imageFormatServices.createImageGroup(vm.newImageGroup.ImageGroupTypeNameKey, vm.newImageGroup.ImageGroupName)
            .success(function (data, status, headers, config) {

                vm.newImageGroup.IsSending = false;
                vm.newImageGroup.SendingComplete = true;

                if (data.isSuccess) {

                    vm.newImageGroup.Results.IsSuccess = true;
                    vm.newImageGroup.Results.Message = "Group created!";
                    vm.loadImageFormatsPanel();
                }
                else {
                    vm.newImageGroup.Results.IsSuccess = false;
                    vm.newImageGroup.Results.Message = data.ErrorMessage;
                }

            })
                .error(function (data, status, headers, config) {

                    vm.newImageGroup.Results.IsSuccess = false;

                    //vm.clearInvitationForm();

                    vm.newImageGroup.IsSending = false;
                    vm.newImageGroup.SendingComplete = true;
                    vm.newImageGroup.Results.Message = "An error occurred while attempting to use the service...";
                })
        }





        /*============================================
         * Format CREATE
         ============================================*/


        vm.newImageFormat =
             {

                 ImageGroupTypeName: null,
                 ImageGroupTypeNameKey: null,

                 ImageGroupNameKey: null,
                 ImageGroupName: null,

                 ImageFormatName: null,

                 Gallery: false,
                 Listing: false,

                 ShowAdvanced: false,
                 VariableWidth: false,

                 Width: null,
                 Height: null,


                 // Service Processing --------

                 IsSending: false,
                 SendingComplete: false,

                 Results: {
                     IsSuccess: false,
                     Message: null
                 },

                 UpdateGroupType: function (groupType, groupTypeNameKey) {
                     this.Clear();
                     this.ImageGroupTypeName = groupType;
                     this.ImageGroupTypeNameKey = groupTypeNameKey;

                 },

                 UpdateGroup: function (group, groupNameKey) {
                     this.ImageGroupName = group;
                     this.ImageGroupNameKey = groupNameKey;
                 },

                 // Cleanup Routine(s) ----------

                 Retry: function () {
                     this.IsSending = false;
                     this.SendingComplete = false;
                 },

                 Clear: function () {

                     //Debug.trace("Clearing new format form data.");

                     this.ImageGroupTypeName = null;
                     this.ImageGroupTypeNameKey = null;
                     this.ImageGroupName = null;
                     this.ImageGroupNameKey = null;

                     this.ImageFormatName = null;

                     this.Gallery = false;
                     this.Listing = false;

                     this.ShowAdvanced = false;
                     this.VariableWidth = false;

                     this.Width = null;
                     this.Height = null;

                     this.IsSending = false;
                     this.SendingComplete = false;

                     this.Results.IsSuccess = false;
                     this.Results.Message = null;
                 }
             }

        vm.showAdvancedImageOptions = function ()
        {
            vm.newImageFormat.ShowAdvanced = true;
        }
        vm.hideAdvancedImageOptions = function ()
        {
            vm.newImageFormat.ShowAdvanced = false;
        }

        vm.setGalleryValue = function(isGallery)
        {
            vm.newImageFormat.Gallery = isGallery;

            if (isGallery)
            {
                vm.newImageFormat.Listing = false;
            }
        }

        vm.setListingValue = function (isListing) {
            vm.newImageFormat.Listing = isListing;

            if (isListing) {
                vm.newImageFormat.Gallery = false;
            }
        }

        vm.setVariableWidthValue = function (isVariable) {
            vm.newImageFormat.VariableWidth = isVariable;
            if (isVariable) {
                vm.newImageFormat.Width = 0;
            }
            else {
                vm.newImageFormat.Width = null;
            }
        }


        vm.initializeCreateImageFormat = function () {
            vm.newImageFormat.Clear();
        }

        vm.createImageFormat = function () {

            vm.newImageFormat.IsSending = true;

            //Debug.trace("Creating image group...");

            imageFormatServices.createImageFormat(vm.newImageFormat.ImageGroupTypeNameKey, vm.newImageFormat.ImageGroupNameKey, vm.newImageFormat.ImageFormatName, vm.newImageFormat.Width, vm.newImageFormat.Height, vm.newImageFormat.Listing, vm.newImageFormat.Gallery)
            .success(function (data, status, headers, config) {

                vm.newImageFormat.IsSending = false;
                vm.newImageFormat.SendingComplete = true;

                if (data.isSuccess) {

                    vm.newImageFormat.Results.IsSuccess = true;
                    vm.newImageFormat.Results.Message = "Format created!";
                    vm.loadImageFormatsPanel();
                }
                else {
                    vm.newImageFormat.Results.IsSuccess = false;
                    vm.newImageFormat.Results.Message = data.ErrorMessage;
                }

            })
                .error(function (data, status, headers, config) {

                    vm.newImageFormat.Results.IsSuccess = false;


                    vm.newImageFormat.IsSending = false;
                    vm.newImageFormat.SendingComplete = true;
                    vm.newImageFormat.Results.Message = "An error occurred while attempting to use the service...";
                })
        }




        /*=====================================
         * Manage/Delete Image Groups
         ===================================*/

        vm.imageGroupDetails =
         {
             ImageGroupTypeNameKey: null,
             ImageGroupName: null,
             ImageGroupNameKey: null,

             // Service Processing --------

             IsSending: false,
             SendingComplete: false,

             Results: {
                 IsSuccess: false,
                 Message: null
             },


             Setup: function (parentIndex, childIndex) {

                 this.ImageGroupTypeNameKey = vm.imageGroupTypes[parentIndex].ImageGroupTypeNameKey;
                 this.ImageGroupName = vm.imageGroupTypes[parentIndex].ImageGroups[childIndex].ImageFormatGroupName;
                 this.ImageGroupNameKey = vm.imageGroupTypes[parentIndex].ImageGroups[childIndex].ImageFormatGroupNameKey;

                 this.IsSending = false;
                 this.SendingComplete = false;

                 this.Results.IsSuccess = false;
                 this.Results.Message = null;
             },
         }

        vm.deleteImageGroup = function () {

            vm.imageGroupDetails.IsSending = true;

            //Debug.trace("Deleting group...");

            imageFormatServices.deleteImageGroup(vm.imageGroupDetails.ImageGroupTypeNameKey, vm.imageGroupDetails.ImageGroupNameKey)
            .success(function (data, status, headers, config) {

                vm.imageGroupDetails.IsSending = false;
                vm.imageGroupDetails.SendingComplete = true;

                if (data.isSuccess) {

                    vm.imageGroupDetails.Results.IsSuccess = true;
                    vm.imageGroupDetails.Results.Message = "Group deleted!";
                    vm.loadImageFormatsPanel();
                }
                else {
                    vm.imageGroupDetails.Results.IsSuccess = false;
                    vm.imageGroupDetails.Results.Message = data.ErrorMessage;
                }

            })
            .error(function (data, status, headers, config) {
                vm.imageGroupDetails.Results.IsSuccess = false;

                vm.imageGroupDetails.IsSending = false;
                vm.imageGroupDetails.SendingComplete = true;
                vm.imageGroupDetails.Results.Message = "An error occurred while attempting to use the service...";
            })
        }

        /*=====================================
        * Manage/Delete Image Formats
        ===================================*/

        vm.imageFormatDetails =
         {
             ImageGroupTypeNameKey: null,
             ImageGroupNameKey: null,
             ImageFormatName: null,
             ImageFormatNameKey: null,

             // Service Processing --------

             IsSending: false,
             SendingComplete: false,

             Results: {
                 IsSuccess: false,
                 Message: null
             },


             Setup: function (parentParentIndex, parentIndex, childIndex) {

                 this.ImageGroupTypeNameKey = vm.imageGroupTypes[parentParentIndex].ImageGroupTypeNameKey;
                 this.ImageGroupNameKey = vm.imageGroupTypes[parentParentIndex].ImageGroups[parentIndex].ImageFormatGroupNameKey;
                 this.ImageFormatName = vm.imageGroupTypes[parentParentIndex].ImageGroups[parentIndex].ImageFormats[childIndex].ImageFormatName;
                 this.ImageFormatNameKey = vm.imageGroupTypes[parentParentIndex].ImageGroups[parentIndex].ImageFormats[childIndex].ImageFormatNameKey;

                 this.IsSending = false;
                 this.SendingComplete = false;

                 this.Results.IsSuccess = false;
                 this.Results.Message = null;
             },
         }

        vm.deleteImageFormat = function () {

            vm.imageFormatDetails.IsSending = true;

            //Debug.trace("Deleting format...");

            imageFormatServices.deleteImageFormat(vm.imageFormatDetails.ImageGroupTypeNameKey, vm.imageFormatDetails.ImageGroupNameKey, vm.imageFormatDetails.ImageFormatNameKey)
            .success(function (data, status, headers, config) {

                vm.imageFormatDetails.IsSending = false;
                vm.imageFormatDetails.SendingComplete = true;

                if (data.isSuccess) {

                    vm.imageFormatDetails.Results.IsSuccess = true;
                    vm.imageFormatDetails.Results.Message = "Format deleted!";
                    vm.loadImageFormatsPanel();
                }
                else {
                    vm.imageFormatDetails.Results.IsSuccess = false;
                    vm.imageFormatDetails.Results.Message = data.ErrorMessage;
                }

            })
            .error(function (data, status, headers, config) {
                vm.imageFormatDetails.Results.IsSuccess = false;

                vm.imageFormatDetails.IsSending = false;
                vm.imageFormatDetails.SendingComplete = true;
                vm.imageFormatDetails.Results.Message = "An error occurred while attempting to use the service...";
            })
        }




        /* ==========================================

            END IMAGE FORMATS METHODS

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
               CONTROLLER ACTIVATION
           ==========================================*/

        activate();

        function activate(){


            //Account Roles (used for the logged in Account user, to check Roles accesability
            vm.userRoles = JSON.parse(CoreServiceSettings_AccountUsers_RolesList);

            // Load local profile for the platfor user.
            vm.currentUserProfile = JSON.parse(CurrentUserProfile);
            currentUserRoleIndex = vm.userRoles.indexOf(vm.currentUserProfile.Role) //<-- Role will indicate what editing capabilites are available.

            //For <legal-footer></legal-footer>
            vm.termsLink = termsLink;
            vm.privacyLink = privacyLink;
            vm.acceptableUseLink = acceptableUseLink;
            vm.serviceAgreement = serviceAgreement;
            vm.theCurrentYear = new Date().getFullYear();


            if (vm.currentUserProfile.Id == "") {
                //Log user out if empty
                window.location.replace("/login");
            }

            //Update user profile info in case of role updates
            updateCurrentUserProfile();
            // Refresh the profile every 45 seconds (if Role is updated, new editing capabilites will light up for the user)
            setInterval(function () { updateCurrentUserProfile() }, 320000);

            vm.getAccount();
            

            //Debug.trace('settingsIndexController activation complete');


            //Bool: Checks if the users role is allowed
            vm.checkRole = function (lowestRoleAllowed) {

                //Debug.trace(vm.userRoles);

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

