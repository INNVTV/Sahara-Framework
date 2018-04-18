(function () {
    'use strict';

    var controllerId = 'tabletsController';

    // Inject into main app module:
    angular.module('app')
        .controller(controllerId, [
            'accountServices',
            'categoryServices',
            'searchServices',
            'productServices',
            'propertyServices',
            'leadServices',
            '$scope',
            '$location',
            '$window',
            '$routeParams',
            '$timeout',
             tabletsController
    ]);

    function tabletsController(accountServices, categoryServices, searchServices, productServices, propertyServices, leadServices, $scope, $location, $window, $routeParams, $timeout) {

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


        //Instantiate Controller as ViewModel
        var vm = this;

        //Default Properties: =============================
        vm.title = 'tabletsController';
        vm.activate = activate;



        vm.loading = true;
        vm.accountNameKey = null;
        vm.apiDomain = null;
        vm.account = null;

        
        vm.fullyQualifiedName = "";
        vm.nameKey = "";

        vm.pageType = "categoryList"; //<-- home, category, productlist, productdetails

        //vm.categoryPageType = " categories, category, subcategory, subsubcategory, subsubsubcateory
        //vm.categorizationType = null;
        //vm.subcategorizationType = null;
        //vm.categorizationSearchFilter = null;


        /* =============================================
         * 
         *   PAGINATION
         * 
         =============================================*/

        vm.amountToSkip = 0;
        vm.resultsAvailable = 0;
        vm.amountPerPage = 36;
        vm.searchingMore = false;

        $(window).scroll(function () {
            if ($(window).scrollTop() + $(window).height() == $(document).height()) {

                if (vm.amountToSkip < vm.resultsAvailable && vm.productResults != null && vm.searchingMore == false) {

                    vm.searchingMore = true;

                    searchServices.searchProducts(vm.account.accountNameKey, vm.apiDomain, '', "locationPath eq '" + vm.currentLocationPath + "'", vm.currentOrdering, vm.amountToSkip, vm.amountPerPage)
                    .success(function (data, status, headers, config) {

                        //vm.searchQueryCompleted = vm.searchQuery;
                        vm.searchingMore = false;
                        vm.resultsAvailable = data.count;

                        //vm.paginationInfo.lastResultCount = data.Results.length;


                        vm.amountToSkip = vm.amountToSkip + data.items.length;

                        //Append the data
                        //vm.products.Results = vm.products.Results.concat(data.Results); //= .push(data.Results);// = [].concat(vm.searchResults, data);
                        vm.productResults = vm.productResults.concat(data.items);


                    })
                    .error(function (data, status, headers, config) {

                    })
                }
            }
        });


        /* ==========================================

               GET ACCOUNT SETTINGS

           ==========================================*/


        //DAISY CHAIN 1 ---------------------
        vm.getAccountSettings = function () {

            accountServices.getAccount(vm.accountNameKey, vm.apiDomain)
            .success(function (data, status, headers, config) {

                vm.account = data.account;
                vm.getCategories(false);
                vm.getSortables();

                document.title = vm.account.accountName;

                //vm.homeOrdering = "dateCreated desc";
                //vm.partialOrdering = "dateCreated desc";
                vm.currentOrdering = "dateCreated desc";


            })
            .error(function (data, status, headers, config) {

            })
        }

        /* ==========================================

               MANAGE SORTING

         ==========================================*/

        vm.getSortables = function () {
            searchServices.getSortables(vm.account.accountNameKey, vm.apiDomain)
            .success(function (data, status, headers, config) {

                vm.sortables = data.sortables;
                vm.sortables.shift(); //<-- shift removes the first item from the array (relevance)
                vm.currentSelectedSortable = vm.sortables[0];

            })
            .error(function (data, status, headers, config) {

            })
        }

        vm.updateSorting = function (sortable) {
            vm.currentSelectedSortable = sortable;
            vm.currentOrdering = sortable.orderByString;
            vm.loadProducts(vm.currentLocationPath);
        }


        /* ==========================================

                 CATEGORIES

        ==========================================*/



        //DAISY CHAIN 2 ---------------------
        vm.getCategories = function(reload)
        {
            if (!reload)
            {
                $("#categoriesDiv").hide();
            }
            

            categoryServices.getCategories(vm.account.accountNameKey, vm.apiDomain)
            .success(function (data, status, headers, config) {

                //vm.loading = false;
                vm.categories = data.categories;
               
                if (!reload) {
                    vm.categoryPageType = "categories";

                    //Load up properties
                    vm.getFeaturedProperties();

                    vm.loading = false;
                    $("#categoriesDiv").fadeIn(400);
                }


            })
            .error(function (data, status, headers, config) {

            })
        }



        vm.loadSubcategory = function (path) {

            vm.loading = true;
            $("#categoriesDiv").hide();

            if (vm.categoryPageType == "categories") {
                $("#stickyNav").hide();
                vm.categoryPageType = "category";
            }
            else if (vm.categoryPageType == "category") {
                vm.categoryPageType = "subcategory";
            }
            else if (vm.categoryPageType == "subcategory") {
                vm.categoryPageType = "subsubcategory";
            }
            else if (vm.categoryPageType == "subsubcategory") {
                vm.categoryPageType = "subsubsubcategory";
            }
           
            categoryServices.getCategorization(vm.account.accountNameKey, vm.apiDomain, vm.categoryPageType, path)
            .success(function (data, status, headers, config) {
              

                if (vm.categoryPageType == "category") {
                    vm.category = data.category;
                    $("#stickyNav").fadeIn(400);

                    if(vm.category.subcategories.length == 0)
                    {
                        //load products
                        vm.loadProducts(vm.category.fullyQualifiedName);
                    }
                    else {
                        vm.loading = false;
                    }
                }
                else if (vm.categoryPageType == "subcategory") {
                    vm.subcategory = data.subcategory;

                    if (vm.subcategory.subsubcategories.length == 0) {
                        //load products
                        vm.loadProducts(vm.subcategory.fullyQualifiedName);
                    }
                    else
                    {
                        vm.loading = false;
                    }
                }
                else if (vm.categoryPageType == "subsubcategory") {
                    vm.subsubcategory = data.subsubcategory;
                    if (vm.subsubcategory.subsubsubcategories.length == 0) {
                        //load products
                        vm.loadProducts(vm.subsubcategory.fullyQualifiedName);
                    }
                    else {
                        vm.loading = false;
                    }
                }
                else if (vm.categoryPageType == "subsubsubcategory") {
                    vm.subsubsubcategory = data.subsubsubcategory;
                    //load products
                    vm.loadProducts(vm.subsubsubcategory.fullyQualifiedName);
                }

                
                $("#categoriesDiv").fadeIn(400);

            })
            .error(function (data, status, headers, config) {

            })
        }

        vm.goBack = function (path) {

            vm.pageType = "categoryList";
            vm.productResults = null;

            $("#categoriesDiv").hide();

            if (vm.categoryPageType == "subsubsubcategory") {
                vm.categoryPageType = "subsubcategory";
            }
            else if (vm.categoryPageType == "subsubcategory") {
                vm.categoryPageType = "subcategory";
            }
            else if (vm.categoryPageType == "subcategory") {
                vm.categoryPageType = "category";
            }
            else if (vm.categoryPageType == "category") {
                vm.categoryPageType = "categories";

                $("#stickyNav").fadeOut(400);

                //We reload at the top level as it is the only level that does not get a reload opportunity
                vm.getCategories(true);
            }

            $("#categoriesDiv").fadeIn(400);
        }


        vm.goBackFromDetails = function () {

            vm.productDetails = null;
            vm.product = null;

            vm.pageType = "productList";
            $("#productListingDiv").fadeIn(400);


            /*
            vm.productResults = null;

            $("#categoriesDiv").hide();

            if (vm.categoryPageType == "subsubsubcategory") {
                vm.categoryPageType = "subsubcategory";
            }
            else if (vm.categoryPageType == "subsubcategory") {
                vm.categoryPageType = "subcategory";
            }
            else if (vm.categoryPageType == "subcategory") {
                vm.categoryPageType = "category";
            }
            else if (vm.categoryPageType == "category") {
                vm.categoryPageType = "categories";
            }

            $("#categoriesDiv").fadeIn(400);*/
        }


        /* ==========================================

               PRODUCTS

        ==========================================*/



        /* ------------- LISTING ------------------*/


        vm.productResults = null;
        vm.currentLocationPath = null;

        vm.loadProducts = function (locationPath) {

            vm.currentLocationPath = locationPath;

            //----Reset---
            vm.amountToSkip = 0;

            $("#productListingDiv").hide();

            vm.pageType = "productList";
            vm.loading = true;

            var query = "";
            var filter = "locationPath eq '" + locationPath + "'";

            searchServices.searchProducts(vm.account.accountNameKey, vm.apiDomain, query, filter, vm.currentOrdering, 0, vm.amountPerPage)
               .success(function (data, status, headers, config) {

                   vm.productResults = data.items;

                   vm.amountToSkip = vm.amountToSkip + vm.amountPerPage;
                   vm.resultsAvailable = data.count;

                   vm.loading = false;
                   $("#productListingDiv").fadeIn(400);

               })
  
        }



        /* ------------- DETAILS ------------------*/


        vm.loadProductDetails = function (fullyQualifiedName) {

            $("#productListingDiv").fadeOut(400);

            vm.productDetails = null;
            vm.product = null;

            vm.loading = true;


            productServices.getProductDetails(vm.account.accountNameKey, vm.apiDomain, fullyQualifiedName)
               .success(function (data, status, headers, config) {

                   vm.pageType = "productDetails";

                   vm.productDetails = data;
                   vm.product = data.item;

                   vm.loading = false;
                   $("#productDetailsDiv").fadeIn(400);

               })

        }













        /* ==========================================
               RE-ROUTING & MENU
        ==========================================*/

        //vm.selectedItem = null;




        vm.loadProduct = function (path) {

            var pathParams = path.split("/");

            vm.fullyQualifiedName = path;
            vm.pageType = "product";

            vm.nameKey = pathParams[pathParams.length];

            $location.update_path('details/' + path, true);
            //vm.updateBreadcrumbs(path);

            //Debug.trace("pre pop:" + pathParams);
            pathParams.pop();
            //Debug.trace("post pop:" + pathParams);

            if (pathParams[0]) {

                vm.categorizationType = "category";
                vm.subcategorizationType = "subcategory";
                vm.nameKey = pathParams[0];
                vm.categorizationSearchFilter = "categoryNameKey eq '" + pathParams[0] + "'";



                //Load category item into backstack and list
                for (var x = 0; x < vm.categories.items.length; x++) {
                    //Debug.trace("Does " + vm.categories.items[x].nameKey + "==" + route[i] + "?");
                    if (vm.categories.items[x].nameKey == vm.nameKey) {

                        //menuItem = vm.categories.items[x];

                        vm.category.path = vm.categories.items[x].fullyQualifiedName;
                        vm.category.items = vm.categories.items[x].subcategories;
                        vm.categories.selectedItem = vm.categories.items[x];
                        vm.category.name = vm.categories.items[x].name;
                        vm.category.image = vm.categories.items[x].images.default.thumbnail.url;
                        vm.category.nameKey = vm.categories.items[x].nameKey;
                    }
                }

                if (vm.category.items.length > 0) {
                    $("#slider0").hide();
                    $("#slider1").show();
                }

                // if (vm.category.items.length == 1) {
                //vm.category.selectedItem = vm.category.items[0];
                // }

            }
            if (pathParams[1]) {

                vm.categorizationType = "subcategory";
                vm.subcategorizationType = "subsubcategory";
                vm.nameKey = pathParams[1];
                vm.categorizationSearchFilter += " and subcategoryNameKey eq '" + pathParams[1] + "'";


                if (vm.category.items != null)
                {
                    //Load subcategory item into backstack and list
                    for (var x = 0; x < vm.category.items.length; x++) {
                        //Debug.trace("Does " + vm.categories.items[x].nameKey + "==" + route[i] + "?");
                        if (vm.category.items[x].nameKey == vm.nameKey) {

                            // menuItem = vm.categories.items[x];

                            vm.subcategory.path = vm.category.items[x].fullyQualifiedName;
                            vm.subcategory.items = vm.category.items[x].subsubcategories;
                            vm.category.selectedItem = vm.category.items[x];
                            vm.subcategory.name = vm.category.items[x].name;
                            vm.subcategory.image = vm.category.items[x].images.default.thumbnail.url;
                            vm.subcategory.nameKey = vm.category.items[x].nameKey;
                        }
                    }
                }


                if (vm.subcategory.items != null)
                {
                    if (vm.subcategory.items.length > 0) {
                        $("#slider1").hide();
                        $("#slider2").show();
                    }
                }


                //if (vm.subcategory.items.length == 1) {
                //vm.subcategory.selectedItem = vm.subcategory.items[0];
                //}


            }
            if (pathParams[2]) {

                vm.categorizationType = "subsubcategory";
                vm.subcategorizationType = "subsubsubcategory";
                vm.nameKey = pathParams[2];
                vm.categorizationSearchFilter += " and subsubcategoryNameKey eq '" + pathParams[2] + "'";



                //Load subsubcategory item into backstack and list
                if (vm.subcategory.items != null)
                {
                    for (var x = 0; x < vm.subcategory.items.length; x++) {
                        //Debug.trace("Does " + vm.categories.items[x].nameKey + "==" + route[i] + "?");
                        if (vm.subcategory.items[x].nameKey == vm.nameKey) {

                            // menuItem = vm.categories.items[x];

                            vm.subsubcategory.path = vm.subcategory.items[x].fullyQualifiedName;
                            vm.subsubcategory.items = vm.subcategory.items[x].subsubsubcategories;
                            vm.subcategory.selectedItem = vm.subcategory.items[x];
                            vm.subsubcategory.name = vm.subcategory.items[x].name;
                            vm.subsubcategory.image = vm.subcategory.items[x].images.default.thumbnail.url;
                            vm.subsubcategory.nameKey = vm.subcategory.items[x].nameKey;
                        }
                    }
                }



                if (vm.subsubcategory.items != null)
                {
                    if (vm.subsubcategory.items.length > 0) {
                        $("#slider2").hide();
                        $("#slider3").show();


                        //if (vm.subsubcategory.items.length == 1)
                        // {
                        // vm.subsubcategory.selectedItem = vm.subsubcategory.items[0];
                        //}
                    }
                }



            }
            if (pathParams[3]) {

                vm.categorizationType = "subsubsubcategory";
                vm.subcategorizationType = null;
                vm.nameKey = pathParams[3];
                vm.categorizationSearchFilter += " and subsubsubcategoryNameKey eq '" + pathParams[3] + "'";


                //Load subsubcategory item into backstack and list

                if (vm.subsubcategory.items != null)
                {
                    for (var x = 0; x < vm.subsubcategory.items.length; x++) {
                        //Debug.trace("Does " + vm.categories.items[x].nameKey + "==" + route[i] + "?");
                        if (vm.subsubcategory.items[x].nameKey == vm.nameKey) {

                            // menuItem = vm.categories.items[x];

                            vm.subsubsubcategory.path = vm.subsubcategory.items[x].fullyQualifiedName;
                            //vm.subsubsubcategory.items = vm.subsubcategory.items[x].subsubsubcategories;
                            vm.subsubcategory.selectedItem = vm.subsubcategory.items[x];
                            vm.subsubsubcategory.name = vm.subsubcategory.items[x].name;
                            vm.subsubsubcategory.image = vm.subsubcategory.items[x].images.default.thumbnail.url;
                            vm.subsubsubcategory.nameKey = vm.subsubcategory.items[x].nameKey;
                        }
                    }
                }


                //if (vm.subsubsubcategory.items.length == 1) {
                // vm.subsubsubcategory.selectedItem = vm.subsubsubcategory.items[0];
                //}
            }

            vm.getProduct();
            
            

        }



        /* ==========================================

               END MENU

        ==========================================*/













        /*==========================================================================
         * 
         * 
         *    PRODUCT DETAILS (START)
         * 
         * 
         =========================================================================*/

        /* ==========================================
               Properties
        ==========================================*/

        // ------ Listings -------

        vm.featuredProperties = [];

        vm.getFeaturedProperties = function () {

            propertyServices.getPropertiesFiltered(vm.accountNameKey, vm.apiDomain, 'featured', 'listingsOnly')
            .success(function (data, status, headers, config) {

                vm.featuredProperties = data.properties;
                vm.getDetailProperties();
            })
            .error(function (data, status, headers, config) {

            })
        }

        // ------- Details -------

        vm.detailProperties = [];

        vm.getDetailProperties = function () {

            propertyServices.getProperties(vm.accountNameKey, vm.apiDomain, 'details')
            .success(function (data, status, headers, config) {

                vm.detailProperties = data.properties;

                //Remove items already accounted for in featured:
                for (var i = 0, len = vm.featuredProperties.length; i < len; i++) {
                    for (var j = 0, len2 = vm.detailProperties.length; j < len2; j++) {
                        if (vm.featuredProperties[i].propertyNameKey === vm.detailProperties[j].propertyNameKey) {
                            vm.detailProperties.splice(j, 1);
                            len2 = vm.detailProperties.length;
                        }
                    }
            }

            })
            .error(function (data, status, headers, config) {

            })
        }


        /* ==========================================
               SUBMIT LEAD
           ==========================================*/

        vm.salesLead = {
            firstName: null,
            lastName: null,
            companyName: null,
            phone: null,
            email: null,
            comments: null,
            productName: null,
            productId: null,
            fullyQualifiedName: null,
            locationPath: null,
            origin: null,
            ipAddress: null,
            userId: null,
            userName: null,

            IsSending: false,
            IsSuccess: null,
            ErrorMessage: null,

            Reset: function () {

                this.IsSending = false;
                this.IsSuccess = null;
                this.ErrorMessage = null;
            }
        }

        vm.initiateNewSalesLead = function () {
            

            vm.salesLead.Reset();

            vm.salesLead.firstName = null;
            vm.salesLead.lastName = null;
            vm.salesLead.companyName = null;
            vm.salesLead.phone = null;
            vm.salesLead.email = null;
            vm.salesLead.comments = null;
            vm.salesLead.productName = vm.product.name;
            vm.salesLead.productId = vm.product.id;
            vm.salesLead.fullyQualifiedName = vm.product.fullyQualifiedName;
            vm.salesLead.locationPath = vm.product.locationPath;
            vm.salesLead.origin = "site";
            vm.salesLead.userId = "",
            vm.salesLead.userName = ""

            try {
                findIP();
            }
            catch (err) {
                //
            }
            
        }

        vm.submitLead = function () {

            vm.salesLead.IsSending = true;

            if (vm.salesLead.phone == null && vm.salesLead.email == null) {
                vm.salesLead.IsSending = false;
                vm.salesLead.IsSuccess = false;
                vm.salesLead.ErrorMessage = "Please include a phone number or email!";
            }
            else {

                //setTimeout(function () { $('#leadSuccessMessage').slideUp(); }, 120);

                leadServices.submitLead(vm.account.accountNameKey, vm.apiDomain, vm.salesLead.firstName, vm.salesLead.lastName, vm.salesLead.companyName, vm.salesLead.phone, vm.salesLead.email, vm.salesLead.comments, vm.salesLead.productName, vm.salesLead.productId, vm.salesLead.fullyQualifiedName, vm.salesLead.locationPath, vm.salesLead.origin, ipOut, vm.salesLead.userId, vm.salesLead.userName)
                    .success(function (data, status, headers, config) {

                        $timeout(function () {
                            vm.salesLead.IsSending = false;
                            vm.salesLead.IsSuccess = true;
                            $timeout($('#leadSuccessMessage').fadeIn(), 780);
                        }, 260);



                        //Debug.trace(data);
                    })
                    .error(function (data, status, headers, config) {
                        vm.salesLead.IsSuccess = false;
                        vm.salesLead.ErrorMessage = "Unknown error";
                    })
            }
        }

        /*================================
                 Get IP Address
         ================================*/

        var ipOut;

        function findIP() {
            var myPeerConnection = window.RTCPeerConnection || window.mozRTCPeerConnection || window.webkitRTCPeerConnection; //compatibility for firefox and chrome
            var pc = new myPeerConnection({ iceServers: [] }),
              noop = function () { },
              localIPs = {},
              ipRegex = /([0-9]{1,3}(\.[0-9]{1,3}){3}|[a-f0-9]{1,4}(:[a-f0-9]{1,4}){7})/g,
              key;

            function ipIterate(ip) {
                //if (!localIPs[ip]) onNewIP(ip);
                //localIPs[ip] = true;
                ipOut = ip;
            }
            pc.createDataChannel(""); //create a bogus data channel
            pc.createOffer(function (sdp) {
                sdp.sdp.split('\n').forEach(function (line) {
                    if (line.indexOf('candidate') < 0) return;
                    line.match(ipRegex).forEach(ipIterate);
                });
                pc.setLocalDescription(sdp, noop, noop);
            }, noop); // create offer and set local description
            pc.onicecandidate = function (ice) { //listen for candidate events
                if (!ice || !ice.candidate || !ice.candidate.candidate || !ice.candidate.candidate.match(ipRegex)) return;
                ice.candidate.candidate.match(ipRegex).forEach(ipIterate);
            };


        }


        /* ==========================================
               GET INITIAL CATEGORIZATION PRODUCTS
           ==========================================*/

        vm.product = null;

        vm.getProduct = function () {

            vm.loading = true;
            vm.loadingMobile = true;
            $("#product-body").hide();

            productServices.getProductDetails(vm.account.accountNameKey, vm.apiDomain, vm.fullyQualifiedName)
            .success(function (data, status, headers, config) {

                vm.product = data.item;
                //vm.loading = false;
                
                vm.updateBreadcrumbs(vm.product.fullyQualifiedName, vm.product.name);

                $timeout(function () {
                    $("#product-body").fadeIn(800);
                    vm.loading = false;
                    vm.loadingMobile = false;
                }, 100)
                

            })
            .error(function (data, status, headers, config) {

            })
        }

        /*==========================================================================
         * 
         *    PRODUCT DETAILS (END)
         * 
         =========================================================================*/


        /* ==========================================
           HELPER FUNCTION
        ==========================================*/

        vm.generateMapEmbedUrl = function(lat, long)
        {
            return "https://www.google.com/maps/embed/v1/place?q=" + lat + ", " + long + "&zoom=16&key=" + vm.googleMapsApiKey;
        }


/*
        vm.screenType = "tablet";

        $(window).resize(function () {
            
            vm.updateSize();

        });

        vm.updateSize = function()
        {
            var size = $(window).width()

            if (size < 768) {
                vm.screenType = "phone";
                //Debug.trace("mobile");
                $("#desktopDiv").hide();
                $("#mobileDiv").show();
            }
            else {
                vm.screenType = "tablet";
                //Debug.trace("desktop");
                $("#desktopDiv").show();
                $("#mobileDiv").hide();
            }
        }*/

        /* ==========================================
               CONTROLLER ACTIVATION
           ==========================================*/

        activate();

        function activate(){

            vm.accountNameKey = AccountNameKey;
            vm.apiDomain = ApiDomain;
            vm.googleMapsApiKey = JSON.parse(CoreServiceSettings_GoogleMapsApiKey);
            

            vm.getAccountSettings();
           // $(".nano").nanoScroller();

           // vm.updateSize();
        }

    }

})();

