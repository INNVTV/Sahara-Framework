(function () {
    'use strict';

    var controllerId = 'dataInjectionProductBatchController';

    // Inject into main app module:
    angular.module('app')
        .controller(controllerId, [
            'sharedServices',
            'dataInjectionServices',
            'productServices',
            'categoryServices',
            'accountServices',
            'tagServices',
            'propertiesServices',
            'imageServices',
             dataInjectionProductBatchController
    ]);

    function dataInjectionProductBatchController(sharedServices, dataInjectionServices, productServices, categoryServices, accountServices, tagServices, propertiesServices, imageServices) {

        //Instantiate Controller as ViewModel
        var vm = this;
        vm.account = null;
        vm.maxCategorizationsPerSet = null;
        vm.maxProductsPerSet = null;

        vm.injectionRunning = false;
        //vm.injectionCount = 0;
        vm.outputLog = [];

        vm.injectionSpeed = 26000;
        vm.propertyInjectionSpeed = 45000; // 45 sec per product

        //Default Properties: =============================
        vm.title = 'dataInjectionProductBatchController';
        vm.activate = activate;

        //vm.locationPath = "test";

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


        /*===========================================
         
                    GET ACCOUNT
         
         ============================================*/

        vm.getAccount = function () {
            accountServices.getAccount()
                    .success(function (data, status, headers, config) {
                        vm.account = data;

                        vm.maxCategorizationsPerSet = vm.account.PaymentPlan.MaxCategorizationsPerSet;

                        //vm.maxProductsPerSet = vm.account.PaymentPlan.MaxProductsPerSet;
                        vm.maxProductsPerSet = 10;

                    })
                    .error(function (data, status, headers, config) {

                    })
        }


        /* ==========================================

             PRODUCT INJECTION

        ==========================================*/



        vm.injectProducts = function () {

            vm.injectionRunning = true;

            var _type = "product";
            var _description = null;
            var _instance = null;
            var _success = null;
            var _error = null;
            var _code = null;

            var i = 0, max = vm.maxProductsPerSet, delay = vm.injectionSpeed, run;

            run = function () {

                _description = "Injecting 'Product " + i + "' into '" + vm.locationPath + "'";
                _instance = null;
                _success = null;
                _error = null;
                _code = null;

                productServices.createProduct(vm.locationPath, "Injected Test Product Number " + i, true)
                   .success(function (data, status, headers, config) {

                       //Debug.trace(data);
                       _instance = data.RoleInstance;
                       _success = data.isSuccess;
                       _error = data.ErrorMessage;
                       _code = data.ErrorCode;

                       if (data.isSuccess) {

                           /* PROCESS IMAGES (OFF)
                           //Get random number between 1-10
                           var random = Math.floor(Math.random() * 10) + 1

                           if (random == 2)
                           {
                               //Process images for new product
                               vm.processImage("5678", "Processing thumbnail image for: product " + i, data.SuccessMessage, 'product', 'default', 'thumbnail', 20, 20, 420, 420);
                               vm.processImage("main1", "Processing main image 1 for: Product " + i, data.SuccessMessage, 'product', 'main', 'featured', 97.2972972972973, 376.2162162162162, 758.9189189189187, 1071.4149443561207);
                           }
                           if (random == 4) {
                               //Process images for new product
                               vm.processImage("123", "Processing thumbnail image for: product " + i, data.SuccessMessage, 'product', 'default', 'thumbnail', 20, 20, 420, 420);
                               vm.processImage("main2", "Processing main image 2 for: Product " + i, data.SuccessMessage, 'product', 'main', 'featured', 1.8435703414316114e-13, 1141.6216216216214, 850.0000000000002, 1200.0000000000002);
                           }
                           if (random == 6) {
                               //Process images for new product
                               vm.processImage("1234", "Processing thumbnail image for: product " + i, data.SuccessMessage, 'product', 'default', 'thumbnail', 20, 20, 420, 420);
                               vm.processImage("main3", "Processing main image 3 for: Product " + i, data.SuccessMessage, 'product', 'main', 'featured', 1.8435703414316114e-13, 564.3243243243243, 850.0000000000001, 1200.0000000000002);
                           }
                           if (random == 8) {
                               //Process images for new product
                               vm.processImage("1233", "Processing thumbnail image for: product " + i, data.SuccessMessage, 'product', 'default', 'thumbnail', 20, 20, 420, 420);
                               vm.processImage("main2", "Processing main image 2 for: Product " + i, data.SuccessMessage, 'product', 'main', 'featured', 1.8435703414316114e-13, 564.3243243243243, 850.0000000000001, 1200.0000000000002);
                           }
*/
                       }


                       vm.outputLog.push({
                           type: _type,
                           description: _description,
                           instance: _instance,
                           success: _success,
                           error: _error,
                           code: _code
                       })

                   })
                   .error(function (data, status, headers, config) {

                       _instance = data.RoleInstance;
                       _error = "An error occurred while attempting to use the service...";

                       vm.outputLog.push({
                           type: _type,
                           description: _description,
                           instance: _instance,
                           success: _success,
                           error: _error,
                           code: _code
                       })
                   })


                if (i++ < max) {
                    setTimeout(run, delay);
                }
                else {
                    vm.injectionRunning = false;
                }
            }
            run();

        }


        vm.updateProperties = function () {

            vm.injectionRunning = true;

            var _type = "property";
            var _description = null;
            var _instance = null;
            var _success = null;
            var _error = null;
            var _code = null;

            var i = 0, max = vm.maxProductsPerSet, delay = vm.propertyInjectionSpeed, run;

            run = function () {

                //_description = "Updating properties for: 'Product " + i;
                //_instance = null;
                //_success = null;
                //_error = null;
                //_code = null;

                if(vm.account.AccountNameKey == "kazs123")
                {
                    setTimeout(function () { vm.propertyUpdate(i, 'headline', 'string', 'Test headline 123 456 7689', 'replace') }, 20);
                    setTimeout(function () { vm.propertyUpdate(i, 'modelNumber', 'string', 'M546-Xkjh-3445jffd-45dgdfdgfdfdf', 'replace') }, 100);
                    setTimeout(function () { vm.propertyUpdate(i, 'sku', 'string', '%$^rtrtysf!2132fghHEQSDDV', 'replace') }, 200);



                    setTimeout(function () { vm.propertyUpdate(i, 'brand', 'predefined', '55555555', 'append') }, 400);
                    setTimeout(function () { vm.propertyUpdate(i, 'brand', 'predefined', 'dfgrytryufghgfhfghfghgfhfggh', 'append') }, 600);
                    setTimeout(function () { vm.propertyUpdate(i, 'brand', 'predefined', '4yrfghfghgfhgh', 'append') }, 800);

                    setTimeout(function () { vm.propertyUpdate(i, 'colors', 'swatch', 'Gold', 'append') }, 1000);
                    setTimeout(function () { vm.propertyUpdate(i, 'colors', 'swatch', 'Silver', 'append') }, 1400);

                    setTimeout(function () { vm.propertyUpdate(i, 'metals', 'swatch', '1', 'append') }, 1700);
                    setTimeout(function () { vm.propertyUpdate(i, 'metals', 'swatch', '2', 'append') }, 2000);
                    setTimeout(function () { vm.propertyUpdate(i, 'metals', 'swatch', '3', 'append') }, 2500);
                    setTimeout(function () { vm.propertyUpdate(i, 'metals', 'swatch', '4', 'append') }, 2900);
                    setTimeout(function () { vm.propertyUpdate(i, 'metals', 'swatch', '56', 'append') }, 3400);

                    setTimeout(function () { vm.propertyUpdate(i, 'price', 'number', '123456', 'replace') }, 6700);
                    setTimeout(function () { vm.propertyUpdate(i, 'bedrooms', 'number', '5556665656', 'replace') }, 7300);
                    setTimeout(function () { vm.propertyUpdate(i, 'bathrooms', 'number', '152345656', 'replace') }, 8900);
                    setTimeout(function () { vm.propertyUpdate(i, 'cost', 'number', '58910123456', 'replace') }, 10300);

                    setTimeout(function () { vm.propertyUpdate(i, 'arrivaldate', 'datetime', '01-16-2017+10:48', 'replace') }, 1100);

                    setTimeout(function () { vm.propertyUpdate(i, 'description', 'string', 'Test copy 123 344w6 %@&! 8*&! ghghs Test copy 123 344w6 %@&! 8*&! ghghs Test copy 123 344w6 %@&! 8*&! ghghs Test copy 123 344w6 %@&! 8*&! ghghs Test copy 123 344w6 %@&! 8*&! ghghs Test copy 123 344w6 %@&! 8*&! ghghs Test copy 123 344w6 %@&! 8*&! ghghs Test copy 123 344w6 %@&! 8*&! ghghs Test copy 123 344w6 %@&! 8*&! ghghs Test copy 123 344w6 %@&! 8*&! ghghs Test copy 123 344w6 %@&! 8*&! ghghs Test copy 123 344w6 %@&! 8*&! ghghs Test copy 123 344w6 %@&! 8*&! ghghs Test copy 12', 'replace') }, 12000);
                    setTimeout(function () { vm.propertyUpdate(i, 'history', 'string', 'Test copy 123 344w6 %@&! 8*&! ghghs Test copy 123 344w6 %@&! 8*&! ghghs Test copy 123 344w6 %@&! 8*&! ghghs Test copy 123 344w6 %@&! 8*&! ghghs Test copy 123 344w6 %@&! 8*&! ghghs Test copy 123 344w6 %@&! 8*&! ghghs Test copy 123 344w6 %@&! 8*&! ghghs Test copy 123 344w6 %@&! 8*&! ghghs Test copy 123 344w6 %@&! 8*&! ghghs Test copy 123 344w6 %@&! 8*&! ghghs Test copy 123 344w6 %@&! 8*&! ghghs Test copy 123 344w6 %@&! 8*&! ghghs Test copy 123 344w6 %@&! 8*&! ghghs Test copy 12', 'replace') }, 13000);
                    setTimeout(function () { vm.propertyUpdate(i, 'biography', 'string', 'Test copy 123 344w6 %@&! 8*&! ghghs Test copy 123 344w6 %@&! 8*&! ghghs Test copy 123 344w6 %@&! 8*&! ghghs Test copy 123 344w6 %@&! 8*&! ghghs Test copy 123 344w6 %@&! 8*&! ghghs Test copy 123 344w6 %@&! 8*&! ghghs Test copy 123 344w6 %@&! 8*&! ghghs Test copy 123 344w6 %@&! 8*&! ghghs Test copy 123 344w6 %@&! 8*&! ghghs Test copy 123 344w6 %@&! 8*&! ghghs Test copy 123 344w6 %@&! 8*&! ghghs Test copy 123 344w6 %@&! 8*&! ghghs Test copy 123 344w6 %@&! 8*&! ghghs Test copy 12', 'replace') }, 14000);

                    setTimeout(function () { vm.propertyTagUpdate(i, 'Eight') }, 15000);
                    setTimeout(function () { vm.propertyTagUpdate(i, 'Seven') }, 15500);
                    setTimeout(function () { vm.propertyTagUpdate(i, 'Three') }, 16000);
                    setTimeout(function () { vm.propertyTagUpdate(i, 'Five') }, 16500);
                    setTimeout(function () { vm.propertyTagUpdate(i, 'Six') }, 17000);
                    setTimeout(function () { vm.propertyTagUpdate(i, 'Two') }, 17500);
                    setTimeout(function () { vm.propertyTagUpdate(i, 'Four') }, 18000);
                    setTimeout(function () { vm.propertyTagUpdate(i, 'Uno') }, 18500);
                    setTimeout(function () { vm.propertyTagUpdate(i, 'Ten') }, 19000);
                    setTimeout(function () { vm.propertyTagUpdate(i, 'Nine') }, 19400);

                    setTimeout(function () {

                        productServices.updateProductLocationProperty(vm.locationPath + "/injected-test-product-number-" + i, "Injected Test Product Number " + i, 'location', 'location', 'Test Name', '12360+Riverside', 'Apartment #134', 'Panorama City', 'California', '91423', 'United Arab Emiretes', '34.15690868986518', '-118.40466320514679')
                           .success(function (data, status, headers, config) {

                               //Debug.trace(data);
                               _instance = data.RoleInstance;
                               _success = data.isSuccess;
                               _error = data.ErrorMessage;
                               _code = data.ErrorCode;

                               vm.outputLog.push({
                                   type: "PropertyLocation",
                                   description: 'location',
                                   instance: _instance,
                                   success: _success,
                                   error: _error,
                                   code: _code
                               })

                           })
                           .error(function (data, status, headers, config) {

                               _instance = data.RoleInstance;
                               _error = "An error occurred while attempting to use the service...";

                               vm.outputLog.push({
                                   type: "PropertyLocation",
                                   description: 'location',
                                   instance: _instance,
                                   success: _success,
                                   error: _error,
                                   code: _code
                               })
                           })
                    }, 26000);
                }
                else if(vm.account.AccountNameKey == "kazs456")
                {
                    setTimeout(function () { vm.propertyUpdate(i, 'bitcoins', 'number', '4444', 'replace') }, 20);
                    setTimeout(function () { vm.propertyUpdate(i, 'type', 'predefined', 'Type Test', 'replace') }, 20);

                    setTimeout(function () { vm.propertyTagUpdate(i, 'Eight') }, 50);
                    setTimeout(function () { vm.propertyTagUpdate(i, 'Seven') }, 150);
                    setTimeout(function () { vm.propertyTagUpdate(i, 'Three') }, 200);
                    setTimeout(function () { vm.propertyTagUpdate(i, 'Five') }, 400);
                    setTimeout(function () { vm.propertyTagUpdate(i, 'Six') }, 800);
                    setTimeout(function () { vm.propertyTagUpdate(i, 'Two') }, 1200);
                    setTimeout(function () { vm.propertyTagUpdate(i, 'Four') }, 1300);
                    setTimeout(function () { vm.propertyTagUpdate(i, 'Uno') }, 1400);
                    setTimeout(function () { vm.propertyTagUpdate(i, 'Ten') }, 1600);
                    setTimeout(function () { vm.propertyTagUpdate(i, 'Nine') }, 1800);

                    setTimeout(function () {

                        productServices.updateProductLocationProperty(vm.locationPath + "/injected-test-product-number-" + i, "Injected Test Product Number " + i, 'location', 'dwellingPoint', 'Test Name', '12360+Riverside', 'Apartment #134', 'Panorama City', 'California', '91423', 'United Arab Emiretes', '34.15690868986518', '-118.40466320514679')
                           .success(function (data, status, headers, config) {

                               //Debug.trace(data);
                               _instance = data.RoleInstance;
                               _success = data.isSuccess;
                               _error = data.ErrorMessage;
                               _code = data.ErrorCode;

                               vm.outputLog.push({
                                   type: "PropertyLocation",
                                   description: 'location',
                                   instance: _instance,
                                   success: _success,
                                   error: _error,
                                   code: _code
                               })

                           })
                           .error(function (data, status, headers, config) {

                               _instance = data.RoleInstance;
                               _error = "An error occurred while attempting to use the service...";

                               vm.outputLog.push({
                                   type: "PropertyLocation",
                                   description: 'location',
                                   instance: _instance,
                                   success: _success,
                                   error: _error,
                                   code: _code
                               })
                           })
                    }, 2000);

                }
                else if(vm.account.AccountNameKey == "kazs789")
                {
                    setTimeout(function () { vm.propertyUpdate(i, 'value', 'number', '7689', 'replace') }, 20);
                    setTimeout(function () { vm.propertyUpdate(i, 'description', 'paragraph', 'Top kek yo, top kek yo, TOP KEK - I SAID I SAID TP KEK....SAD!', 'replace') }, 220);
                    setTimeout(function () { vm.propertyUpdate(i, 'how', 'number', 'By Sea', 'replace') }, 320);
                }


                if (i++ < max) {
                    setTimeout(run, delay);
                }
                else {
                    vm.injectionRunning = false;
                }
            }
            run();

        }


        /* ==========================================
           Image Processing
        ==========================================*/

        vm.processImage = function (imageId, description, objectId, objectType, imageGroupNameKey, imageFormatNameKey, top, left, right, bottom) {
            var _type = objectType + "-image";
            var _description = description;
            var _instance = null;
            var _success = null;
            var _error = null;
            var _code = null;

            dataInjectionServices.processImage(imageId, objectType, objectId, imageGroupNameKey, imageFormatNameKey, 'datainjection', 'jpg', 'jpg', top, left, right, bottom)
                   .success(function (data, status, headers, config) {

                       _instance = data.RoleInstance;
                       _success = data.isSuccess;
                       _error = data.ErrorMessage;
                       _code = data.ErrorCode;

                       vm.outputLog.push({
                           type: _type,
                           description: _description,
                           instance: _instance,
                           success: _success,
                           error: _error,
                           code: _code
                       })

                   })
                   .error(function (data, status, headers, config) {

                       _instance = data.RoleInstance;
                       _error = "An error occurred while attempting to use the service...";

                       vm.outputLog.push({
                           type: _type,
                           description: _description,
                           instance: _instance,
                           success: _success,
                           error: _error,
                           code: _code
                       })
                   })
        }
        

        /* ==========================================
           Property Update Processing
        ==========================================*/

        vm.propertyUpdate = function (id, propertyNameKey, propertyTypeNameKey,  propertyValue, updateType) {

            var _type = "property";
            var _description = "Updating " + propertyNameKey + " for product: " + id;
            var _instance = null;
            var _success = null;
            var _error = null;
            var _code = null;

            productServices.updateProductProperty(vm.locationPath + "/injected-test-product-number-" + id, "Injected Test Product Number " + id, propertyTypeNameKey, propertyNameKey, propertyValue, updateType)
                   .success(function (data, status, headers, config) {

                       //Debug.trace(data);
                       _instance = data.RoleInstance;
                       _success = data.isSuccess;
                       _error = data.ErrorMessage;
                       _code = data.ErrorCode;

                       vm.outputLog.push({
                           type: "Property",
                           description: _description,
                           instance: _instance,
                           success: _success,
                           error: _error,
                           code: _code
                       })

                   })
                   .error(function (data, status, headers, config) {

                       _instance = data.RoleInstance;
                       _error = "An error occurred while attempting to use the service...";

                       vm.outputLog.push({
                           type: "Property",
                           description: _description,
                           instance: _instance,
                           success: _success,
                           error: _error,
                           code: _code
                       })
                   })
            
        }


        vm.propertyTagUpdate = function (id, tagName) {

            var _type = "tag";
            var _description = "Adding '" + tagName + "' tag to product: " + id;
            var _instance = null;
            var _success = null;
            var _error = null;
            var _code = null;

            productServices.addProductTag(vm.locationPath + "/injected-test-product-number-" + id, "Injected Test Product Number " + id, tagName)
                   .success(function (data, status, headers, config) {

                       //Debug.trace(data);
                       _instance = data.RoleInstance;
                       _success = data.isSuccess;
                       _error = data.ErrorMessage;
                       _code = data.ErrorCode;

                       vm.outputLog.push({
                           type: "Tag",
                           description: _description,
                           instance: _instance,
                           success: _success,
                           error: _error,
                           code: _code
                       })

                   })
                   .error(function (data, status, headers, config) {

                       _instance = data.RoleInstance;
                       _error = "An error occurred while attempting to use the service...";

                       vm.outputLog.push({
                           type: "Tag",
                           description: _description,
                           instance: _instance,
                           success: _success,
                           error: _error,
                           code: _code
                       })
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

        function activate(){

            // Injected variables from the view (via CoreServices/PlatformSettings)
            //Platform --------------------------------------------
            //vm.TrialDaysHold = CoreServiceSettings_Custodian_TrialHoldDays;
            //vm.CustodianFrequencyDescription = CoreServiceSettings_Custodian_FrequencyDescription;
            //vm.UnverifiedAccountsDaysToHold = CoreServiceSettings_Custodian_UnverifiedAccountsDaysToHold;
            //vm.PlatformWorkerFrequencyDescription = CoreServiceSettings_PlatformWorker_FrequencyDescription;

            //Account Roles (used for the logged in Account user, to check Roles accesability
            vm.userRoles = JSON.parse(CoreServiceSettings_AccountUsers_RolesList);
            vm.getAccount();


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


            //Debug.trace('dataInjectionIndexController activation complete');



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

