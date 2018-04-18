(function () {
    'use strict';

    var controllerId = 'leadsController';

    // Inject into main app module:
    angular.module('app')
        .controller(controllerId, [
            'leadsServices',
            'sharedServices',
            'accountServices',
            'accountSettingsServices',
            'productServices',
            'imageServices',
             leadsController
    ]);

    function leadsController(leadsServices, sharedServices, accountServices, accountSettingsServices, productServices, imageServices) {

        //Instantiate Controller as ViewModel
        var vm = this;

        //Default Properties: =============================
        vm.title = 'leadsController';
        vm.activate = activate;
        vm.maxLeadsPerPage = 50;

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

            GET ACCOUNT

        ==========================================*/


        vm.account = null

        vm.getAccount = function () {

            //Debug.trace('Getting account details...');

            accountServices.getAccount()
                    .success(function (data, status, headers, config) {
                        vm.account = data;
                        // Manage routes for subscribe / upgrade / card so the correct modal can be initiated
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

            GET LABELS

        ==========================================*/

        vm.leadLabel = "New"; //<-- Default
        vm.leadLabels = [];

        vm.getLabels = function () {

            accountSettingsServices.getAccountSettings()
                    .success(function (data, status, headers, config) {
                        vm.leadLabels = data.SalesSettings.LeadLabels;

                    })
                    .error(function (data, status, headers, config) {
                        //
                    })
        }

        vm.updateLeadLabel = function(label)
        {
            vm.leadLabel = label;
            vm.getLeads(label);
        }

        /* ==========================================
               END GET LABELS
        ==========================================*/





        /* ==========================================

            GET LEADS

        ==========================================*/

        vm.salesLeads = [];
        vm.loadingSalesLeads = true;
        vm.lastUsedLabel = null;
        vm.loadingMore = false;
        vm.paginationComplete = false;
        vm.salesLeadsLength = 0;

        vm.getLeads = function(label)
        {            
            var lastPartitionKey = null;
            var lastRowKey = null;

            if (vm.salesLeads.length > 0 && label == vm.lastUsedLabel)
            {
                //paginate
                vm.loadingMore = true;
                lastPartitionKey = vm.salesLeads[vm.salesLeads.length -1].PartitionKey
                lastRowKey = vm.salesLeads[vm.salesLeads.length -1].RowKey
            }
            else if(label != vm.lastUsedLabel)
            {
                //fresh set
                vm.paginationComplete = false;
                vm.loadingSalesLeads = true;
            }
            else {
                //fresh set
                vm.paginationComplete = false;
                vm.loadingSalesLeads = true;
            }

            leadsServices.getSalesLeads(label, vm.maxLeadsPerPage, lastPartitionKey, lastRowKey)
                    .success(function (data, status, headers, config) {

                        if (vm.salesLeads.length > 0 && label == vm.lastUsedLabel)
                        {
                            vm.salesLeads = vm.salesLeads.concat(data); //<-- Append continuation
                            vm.loadingMore = false;
                            if (data.length == 0 || data.length < vm.maxLeadsPerPage)
                            {
                                vm.paginationComplete = true;
                            }
                        }
                        else {
                            vm.salesLeads = data; //<-- Refresh continuations
                        }
                        
                        vm.salesLeadsLength = vm.salesLeads.length;

                        vm.loadingSalesLeads = false;
                        vm.lastUsedLabel = label;

                    })
                    .error(function (data, status, headers, config) {
                        vm.loadingSalesLeads = false;
                    })
        }

        /* ==========================================
               END GET LEADS
        ==========================================*/

        /* ==========================================

            LOAD & MANAGE LEAD DETAILS

        ==========================================*/

        vm.detailsDefaultActiveTab = true;
        vm.moveLeadToLabel = null;
        vm.currentLeadIndex = null;
        vm.showLeadDeleteButton = false;
        vm.leadDetailsMainHeading = "Contact Info";

        vm.getLeadDetail = function (index) {

            vm.detailsDefaultActiveTab = true;

            vm.product = null;
            vm.productImages = null;
            vm.productThumbnail = null;

            vm.cancelPropertyEdit();          

            vm.showLeadDeleteButton = false;
            vm.leadMove.reset();
            vm.moveLeadToLabel = null;

            vm.leadDetail = vm.salesLeads[index];
            vm.currentLeadIndex = index;

            if ($(window).width() <= 990) {
                vm.leadDetailsMainHeading = "Details";
            }
            else {
                vm.leadDetailsMainHeading = "Contact Info";
            }
        }

        
        $(window).resize(function () {


            if ($(window).width() <= 990) {
                vm.leadDetailsMainHeading = "Details";
            }
            else {
                vm.leadDetailsMainHeading = "Contact Info";
            }

        });



        vm.updateMoveLabelToMoveTo = function(label)
        {
            vm.moveLeadToLabel = label;
        }


        //Moving/Deleting
        vm.leadMove =
            {

                Ready: false,
                Complete: false,
                Processing: false,
                IsSuccess: false,
                Message: null,
                Type:null,

                reset: function () {
                    this.Type = null;
                    this.Ready = false,
                    this.Complete = false,
                    this.Processing = false
                    this.IsSuccess = false;
                    this.Message = null;
                },

            }

        vm.moveLead = function(isDelete)
        {
            vm.leadMove.reset();

            vm.leadMove.Processing = true;
            vm.leadMove.Complete = false;


            var moveToLabel = vm.moveLeadToLabel
            vm.leadMove.Type = "Moving";

            if(isDelete)
            {
                moveToLabel = "Deleted"
                vm.leadMove.Type = "Deleting";             
            }


            leadsServices.moveSalesLead(vm.leadDetail.PartitionKey, vm.leadDetail.RowKey, vm.leadLabel, moveToLabel)
                    .success(function (data, status, headers, config) {

                        vm.leadMove.Processing = false;
                        vm.leadMove.Complete = true;

                        if(data.isSuccess)
                        {
                            vm.leadMove.IsSuccess = true;
                            vm.leadMove.Message = "Lead has been moved!";
                            if (isDelete) {
                                vm.leadMove.Message = "Lead has been deleted!";
                            }

                            vm.salesLeads.splice(vm.currentLeadIndex, 1);
                        }
                        else {
                            vm.leadMove.IsSuccess = false;
                            vm.leadMove.Message = data.ErrorMessage;
                        }
                        

                    })
                    .error(function (data, status, headers, config) {
                        vm.leadMove.Processing = false;
                        vm.leadMove.Complete = true;
                        vm.leadMove.IsSuccess = false;
                        vm.leadMove.Message = "An unknown error occurred.";
                    })
        }



        vm.showAdvancedLeadDetailOptions = function () {
            vm.showLeadDeleteButton = true;
        }

        vm.hideAdvancedLeadDetailOptions = function () {
            vm.showLeadDeleteButton = false;
        }

        /* ==========================================
               Load Lead Detail Product
        ==========================================*/

        vm.product = null;
        vm.productImages = null;
        vm.productThumbnail = null;

        vm.loadProduct = function()
        {
            if (vm.product == null)
            {
                vm.loadProductImageRecords();

                productServices.getProductById(vm.leadDetail.ProductID)
                .success(function (data, status, headers, config) {

                    vm.product = data;
                    vm.loadProductImageRecords();

                })
                    .error(function (data, status, headers, config) {


                    })
            }

            
        }

        vm.loadProductImageRecords = function () {

            imageServices.getImageRecordsForObject('product', vm.leadDetail.ProductID, true)
            .success(function (data, status, headers, config) {

                vm.productImages = data;

                for (var i = 0; i < vm.productImages.length; i++) {
                    if (vm.productImages[i].GroupNameKey == 'default')
                    {
                        for (var x = 0; x < vm.productImages[i].ImageRecords.length; x++) {
                            if (vm.productImages[i].ImageRecords[x].FormatNameKey == 'thumbnail') {
                                vm.productThumbnail = vm.productImages[i].ImageRecords[x].Url_sm;
                            }
                        }
                    }
                }

            })
            .error(function (data, status, headers, config) {

            })

        }


        /* ==========================================
               EDIT LEAD DETAILS PROPERTIES
        ==========================================*/

        vm.propertyEditingName = null;
        vm.propertyEditingValue = null;
        vm.propertyEditingErrorMessage = null;

        vm.propertyEditing = false;
        vm.propertyEditProcessing = false;
        vm.propertyEditingComplete = true;
        

        vm.editProperty = function (propertyName, propertyValue)
        {
            vm.propertyEditingName = propertyName;
            vm.propertyEditingValue = propertyValue;
            vm.propertyEditing = true;
            vm.propertyEditProcessing = false;
            vm.propertyEditingComplete = false;
            vm.propertyEditingErrorMessage = null;
        }

        vm.cancelPropertyEdit = function ()
        {
            vm.propertyEditingName = null;
            vm.propertyEditingValue = null;
            vm.propertyEditing = false;
            vm.propertyEditProcessing = false;
            vm.propertyEditingComplete = true;
            vm.propertyEditingErrorMessage = null;
        }

        vm.updateProperty = function()
        {
            vm.propertyEditProcessing = true;

            leadsServices.updateSalesLead(vm.leadDetail.PartitionKey, vm.leadDetail.RowKey, vm.leadLabel, vm.propertyEditingName, vm.propertyEditingValue)
                    .success(function (data, status, headers, config) {

                        vm.propertyEditProcessing = false;
                        vm.propertyEditingComplete = true;

                        if (data.isSuccess) {
                            
                            vm.leadDetail[vm.propertyEditingName] = vm.propertyEditingValue;
                            vm.salesLeads[vm.currentLeadIndex][vm.propertyEditingName] = vm.propertyEditingValue;
                            vm.cancelPropertyEdit();
                        }
                        else {
                            vm.propertyEditingErrorMessage = data.ErrorMessage;
                        }
                    })
                    .error(function (data, status, headers, config) {

                        vm.propertyEditProcessing = false;
                        vm.propertyEditingComplete = true;
                        vm.propertyEditingErrorMessage = "An unknown error occurred.";

                    })
        }


        /* ==========================================
               END LOAD & MANAGE LEAD DETAILS
        ==========================================*/









        /* ==========================================
           CURRENT USER PROFILE
       ==========================================*/

        function updateCurrentUserProfile() {

            //Debug.trace("Refreshing user profile...");

            sharedServices.getCurrentUserProfile()
            .success(function (data, status, headers, config) {

                vm.currentUserProfile = data; //Used to determine what is shown in the view based on user Role.
                currentUserRoleIndex = vm.userRoles.indexOf(data.Role) //<-- use PLATFORM roles, NOT ACCOUNT roles!

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
               Infinite Scroll
           ==========================================*/

        $(window).scroll(function () {
            if ($(window).scrollTop() + $(window).height() == $(document).height()) {

                if (vm.salesLeadsLength >= vm.maxLeadsPerPage) {
                    if(!vm.paginationComplete)
                    {
                        vm.getLeads(vm.lastUsedLabel);
                    }
                    
                }

            }
        });

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
            vm.userRoles = JSON.parse(CoreServiceSettings_AccountUsers_RolesList);

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


            vm.getAccount();
            vm.getLabels();
            vm.getLeads("New");


            //Debug.trace('leadsIndexController activation complete');



            //Bool: Checks if the users role is allowed
            vm.checkRole = function (lowestRoleAllowed) {

                var allowedIndex = vm.userRoles.indexOf(String(lowestRoleAllowed)); //<-- use Platform roles, NOT account roles!

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

