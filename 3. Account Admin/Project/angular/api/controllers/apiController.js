(function () {
    'use strict';

    var controllerId = 'apiController';

    // Inject into main app module:
    angular.module('app')
        .controller(controllerId, [
            'sharedServices',
            'accountServices',
            'categoryServices',
            'searchServices',
            'tagServices',
            'propertiesServices',
            'apiKeyServices',
             apiController
    ]);

    function apiController(sharedServices, accountServices, categoryServices, searchServices, tagServices, propertiesServices, apiKeyServices) {

        //Instantiate Controller as ViewModel
        var vm = this;

        //Default Properties: =============================
        vm.title = 'apiController';
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



        /*============================================================
            OPEN ENDPOINTS SUBMENU
        =======================================================*/

        vm.openEndpointsSubMenu =
            {
                accountButton: true,
                categoriesButton: false,
                productsButton: false,
                browseButton: false,
                searchButton: false,
                attributesButton: false,
                salesButton: false,

                update: function (buttonName) {

                    if (buttonName == 'account') {
                        this.accountButton = true;
                        this.categoriesButton = false;
                        this.productsButton = false;
                        this.browseButton = false;
                        this.searchButton = false;
                        this.attributesButton = false;
                        this.salesButton = false;
                    }
                    if (buttonName == 'categories') {
                        this.accountButton = false;
                        this.categoriesButton = true;
                        this.productsButton = false;
                        this.browseButton = false;
                        this.searchButton = false;
                        this.attributesButton = false;
                        this.salesButton = false;
                    }
                    if (buttonName == 'products') {
                        this.accountButton = false;
                        this.categoriesButton = false;
                        this.productsButton = true;
                        this.browseButton = false;
                        this.searchButton = false;
                        this.attributesButton = false;
                        this.salesButton = false;
                    }
                    if (buttonName == 'browse') {
                        this.accountButton = false;
                        this.categoriesButton = false;
                        this.productsButton = false;
                        this.browseButton = true;
                        this.searchButton = false;
                        this.attributesButton = false;
                        this.salesButton = false;
                    }
                    if (buttonName == 'search') {
                        this.accountButton = false;
                        this.categoriesButton = false;
                        this.productsButton = false;
                        this.browseButton = false;
                        this.searchButton = true;
                        this.attributesButton = false;
                        this.salesButton = false;
                    }
                    if (buttonName == 'attributes') {
                        this.accountButton = false;
                        this.categoriesButton = false;
                        this.productsButton = false;
                        this.browseButton = false;
                        this.searchButton = false;
                        this.attributesButton = true;
                        this.salesButton = false;
                    }
                    if (buttonName == 'sales') {
                        this.accountButton = false;
                        this.categoriesButton = false;
                        this.productsButton = false;
                        this.browseButton = false;
                        this.searchButton = false;
                        this.attributesButton = false;
                        this.salesButton = true;
                    }
                },
            }


        /*============================================================
           SECURE ENDPOINTS SUBMENU
       =======================================================*/

        vm.secureEndpointsSubMenu =
            {
                categoriesButton: true,
                productsButton: false,
                tagsButton: false,

                update: function (buttonName) {

                    if (buttonName == 'categories') {
                        this.categoriesButton = true;
                        this.productsButton = false;
                        this.tagsButton = false;
                    }
                    if (buttonName == 'products') {
                        this.categoriesButton = false;
                        this.productsButton = true;
                        this.tagsButton = false;
                    }
                    if (buttonName == 'tags') {
                        this.categoriesButton = false;
                        this.productsButton = false;
                        this.tagsButton = true;
                    }
                },
            }




        /*============================================================

           CREATE API KEY

        =======================================================*/

        vm.newApiKeyName = "";

        vm.newApiKeyGenerating = false;
        vm.newApiKeyGeneratingComplete = false;
        vm.newApiKeyGeneratingIsSuccess = false;
        vm.newApiKeyGeneratingMessage = "";


        vm.initiateKeyGeneration = function()
        {

            vm.newApiKeyGenerating = false;
            vm.newApiKeyGeneratingComplete = false;
            vm.newApiKeyGeneratingIsSuccess = false;
            vm.newApiKeyGeneratingMessage = "";
        }

        vm.generateApiKey = function () {

            vm.newApiKeyGenerating = true;

            apiKeyServices.generateApiKey(vm.newApiKeyName, '')
                .success(function (data, status, headers, config) {
  
                    vm.newApiKeyName = "";

                    vm.newApiKeyGenerating = false;
                    vm.newApiKeyGeneratingComplete = true;
                    vm.newApiKeyGeneratingIsSuccess = true;
                    vm.newApiKeyGeneratingMessage = data.replace(/\"/g, "");
                    vm.getApiKeys();
                })
                .error(function (data, status, headers, config) {
                    vm.newApiKeyGenerating = false;
                    vm.newApiKeyGeneratingComplete = true;
                    vm.newApiKeyGeneratingIsSuccess = false;
                    vm.newApiKeyGeneratingMessage = "Unknown error.";
                })
        }


        /*============================================================

           MANAGE API KEYS

       =======================================================*/

        vm.keyUpdating = "";

        vm.regenerateApiKey = function(apiKey)
        {
            if(vm.keyUpdating == "") //<--One key at a time!
            {
                vm.keyUpdating = apiKey;

                apiKeyServices.regenerateApiKey(apiKey)
                    .success(function (data, status, headers, config) {
                        
                        vm.keyUpdating = "";
                        vm.getApiKeys();
                    })
                    .error(function (data, status, headers, config) {
                        vm.keyUpdating = "";
                    })
            }

        }

        vm.deleteApiKey = function (apiKey) {
            if (vm.keyUpdating == "") //<--One key at a time!
            {
                vm.keyUpdating = apiKey;
                apiKeyServices.deleteApiKey(apiKey)
                    .success(function (data, status, headers, config) {

                        vm.keyUpdating = "";
                        vm.getApiKeys();
                    })
                    .error(function (data, status, headers, config) {
                        vm.keyUpdating = "";
                    })
            }

        }



        /* ====================================================================
                Categorization Tree & Random Product for Search API Examples
        ======================================================================*/

        //These will be used as exmples in our API calls (if they exist)
        vm.categoryTree = null;
        //--------------------------------------
        vm.exampleCategoryNameKey = null;
        //vm.exampleCategoryID = null;

        vm.exampleSubcategoryNameKey = null;

        vm.exampleSubsubcategoryNameKey = null;

        vm.exampleSubsubsubcategoryNameKey = null;

        vm.productSearchResults = null;
        vm.exampleProductId = null;
        vm.exampleProductFullyQualifiedName = null;
        vm.exampleProductCategoryNameKey = null;

        vm.exampleTagName = null;
        vm.examplePropertyNameKey = null;

        vm.getCategoryTree = function()
        {
            categoryServices.getCategoryTree(false)
                    .success(function (data, status, headers, config) {

                        //This is also a nice way of refreshing the redis caches:
                        vm.categoryTree = data;

                        vm.getRandomProduct();

                        vm.getRandomTag();
                        vm.getRandomProperty();
                        
                        //randomly select items for API example calls

                        //get count of public category and randomly select one
                        var randomCategoryIndex = randomIntFromInterval(0, (vm.categoryTree.length-1))
                        vm.exampleCategoryNameKey = vm.categoryTree[randomCategoryIndex].NameKey;
                        //vm.exampleCategoryID = vm.categoryTree[randomCategoryIndex].ID;

                        //Used for dicerolls to randomly stop after selecting items
                        var stopSub = false;
                        var stopSubSub = false;
                        var stopSubSubSub = false;

                        //Get random subsubsubcategories (if any exist)
                        for (var i = 0; i < vm.categoryTree.length; i++) {

                            if(vm.categoryTree[i].Subcategories.length > 0)
                            {
                                if(!stopSub)
                                {
                                    vm.exampleSubcategoryNameKey = vm.categoryTree[i].Subcategories[0].FullyQualifiedName;
                                }
                                
                                if(randomIntFromInterval(0, 4) == 4)
                                {
                                    stopSub = true;
                                }
                            }

                            for (var x = 0; x < vm.categoryTree[i].Subcategories.length; x++) {

                                if (vm.categoryTree[i].Subcategories[x].Subsubcategories.length > 0) {

                                    if (!stopSubSub) {
                                        vm.exampleSubsubcategoryNameKey = vm.categoryTree[i].Subcategories[x].Subsubcategories[0].FullyQualifiedName;
                                    }

                                    if (randomIntFromInterval(0, 4) == 4) {
                                        stopSubSub = true;
                                    }

                                    
                                }

                                for (var y = 0; y < vm.categoryTree[i].Subcategories[x].Subsubcategories.length; y++) {
                                    if (vm.categoryTree[i].Subcategories[x].Subsubcategories[y].Subsubsubcategories.length > 0) {

                                        if (!stopSubSubSub) {
                                            vm.exampleSubsubsubcategoryNameKey = vm.categoryTree[i].Subcategories[x].Subsubcategories[y].Subsubsubcategories[0].FullyQualifiedName;
                                        }

                                        if (randomIntFromInterval(0, 4) == 4) {
                                            stopSubSubSub = true;
                                        }

                                        
                                    }
                                }
                            }
                        }
                        

                    })
                    .error(function (data, status, headers, config) {
                        //
                    })
        }

        vm.getRandomProduct = function () {

            searchServices.searchProducts("", "", "relevance", 0, 25)
                    .success(function (data, status, headers, config) {

                        vm.productSearchResults = data;

                        //randomly select items for API example calls
                        var randomProductIndex = randomIntFromInterval(0, (vm.productSearchResults.Returned - 1))
                        vm.exampleProductId = vm.productSearchResults.Results[randomProductIndex].Document.id;
                        vm.exampleProductFullyQualifiedName = vm.productSearchResults.Results[randomProductIndex].Document.fullyQualifiedName;
                        vm.exampleProductCategoryNameKey = vm.productSearchResults.Results[randomProductIndex].Document.categoryNameKey;
                    })
                    .error(function (data, status, headers, config) {
                        //
                    })
        }

        vm.getRandomTag = function () {

            tagServices.getTags()
                    .success(function (data, status, headers, config) {

                        vm.tagResults = data;

                        //randomly select item for API example calls
                        var randomTagIndex = randomIntFromInterval(0, (vm.tagResults.length - 1))

                        vm.exampleTagName = vm.tagResults[randomTagIndex];

                    })
                    .error(function (data, status, headers, config) {
                        //
                    })
        }

        vm.getRandomProperty = function () {

            propertiesServices.getProperties()
                    .success(function (data, status, headers, config) {

                        vm.propertyResults = data;

                        //randomly select item for API example calls
                        var randomPropertyIndex = randomIntFromInterval(0, (vm.propertyResults.length - 1))

                        vm.examplePropertyNameKey = vm.propertyResults[randomPropertyIndex].PropertyNameKey;

                    })
                    .error(function (data, status, headers, config) {
                        //
                    })
        }

        function randomIntFromInterval(min, max) {
            return Math.floor(Math.random() * (max - min + 1) + min);
        }







        /* ==========================================
                Controller Properties
        ==========================================*/

        vm.showLoader = false;



        /* ==========================================
               Controller Models
       ==========================================*/




        /* ==========================================
               Base Controller Methods
        ==========================================*/

        vm.account = null

        vm.getAccount = function () {

            //Debug.trace('Getting account details...');

            accountServices.getAccount()
                    .success(function (data, status, headers, config) {
                        vm.account = data;
                        //vm.getAccountCapacity();

                        // Manage routes for subscribe / upgrade / card so the correct modal can be initiated
                        //Debug.trace("Route action: " + vm.routeAction);

                    })
                    .error(function (data, status, headers, config) {
                        //
                    })
        }


        /* ==========================================
               API TOKENS
           ==========================================*/

        vm.apiKeys = null

        vm.getApiKeys = function () {

            //Debug.trace('Getting account details...');

            apiKeyServices.getApiKeys()
                    .success(function (data, status, headers, config) {
                        vm.apiKeys = data;
                        //vm.getAccountCapacity();

                        // Manage routes for subscribe / upgrade / card so the correct modal can be initiated
                        //Debug.trace("Route action: " + vm.routeAction);

                    })
                    .error(function (data, status, headers, config) {
                        //
                    })
        }




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

            // Injected variables from the view (via CoreServices/PlatformSettings)
            //Platform --------------------------------------------
            //vm.TrialDaysHold = CoreServiceSettings_Custodian_TrialHoldDays;
            //vm.CustodianFrequencyDescription = CoreServiceSettings_Custodian_FrequencyDescription;
            //vm.UnverifiedAccountsDaysToHold = CoreServiceSettings_Custodian_UnverifiedAccountsDaysToHold;
            //vm.PlatformWorkerFrequencyDescription = CoreServiceSettings_PlatformWorker_FrequencyDescription;


            vm.getApiKeys();

            //Account Roles (used for the logged in Account user, to check Roles accesability
            vm.userRoles = JSON.parse(CoreServiceSettings_AccountUsers_RolesList);

            //API Services URL
            vm.apiDomain = JSON.parse(CoreServiceSettings_Urls_AccountServiceUri);

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
            //Debug.trace('apiController activation complete');

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

            //Get CategorizationTree for API examples
            vm.getCategoryTree();
        }

    }

})();

