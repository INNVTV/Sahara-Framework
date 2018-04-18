(function () {
    'use strict';

    var controllerId = 'browseController';

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
             browseController
    ]);

    function browseController(accountServices, categoryServices, searchServices, productServices, propertyServices, leadServices, $scope, $location, $window, $routeParams, $timeout) {

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
        vm.title = 'browseController';
        vm.activate = activate;

        vm.loading = true;
        vm.loadingMobile = true; //<-- so we don't interup to menu on desktop by sharing vm.loading
        vm.accountNameKey = null;
        vm.apiDomain = null;
        vm.account = null;

        
        vm.fullyQualifiedName = "";
        vm.nameKey = "";
        vm.pageType = "home"; //<-- home, browse-partial, browse-full, or product
        vm.categorizationType = null;
        vm.subcategorizationType = null;
        vm.categorizationSearchFilter = null;



        /* ==========================================

               BREADCRUMBS

        ==========================================*/

        vm.breadcrumbs = [];


        vm.updateBreadcrumbs = function(fullyQualifiedName, productName)
        {
            vm.breadcrumbs = [];
            var lastAdded = false;

            //Debug.trace("bread:" + fullyQualifiedName);

            if (fullyQualifiedName != null && fullyQualifiedName != '')
            {
                var route = fullyQualifiedName.split("/");

                vm.breadcrumbs.push({ name: 'All', link: '/' })

                var item = null;
                var lastItemPushed = 
                    {
                        fullyQualifiedName: ""
                    }

                //loop through categories and assign, last item is not a link
                for (var i = 0; i < route.length; i++) {

                    //find item in vm.categories
                    if (i == 0 && vm.categories != undefined) {
                        for (var x = 0; x < vm.categories.items.length; x++) {
                            //Debug.trace("C (" + x + ") Does " + vm.categories.items[x].nameKey + "==" + route[i] + "?");
                            if (vm.categories.items[x].nameKey == route[i]) {
                                item = vm.categories.items[x];
                                //Debug.trace("item subcats 1: " + item.subcategories.length);
                            }
                        }
                    }
                    else if (i == 1 && item != null && item.subcategories != undefined) {
                        var subcategories = item.subcategories;
                        for (var x = 0; x < subcategories.length; x++) {
                            //Debug.trace("SC (" + x + ") Does " + subcategories[x].nameKey + "==" + route[i] + "?");
                            if (subcategories[x].nameKey == route[i]) {
                                item = subcategories[x];
                            }
                        }
                    }
                    else if (i == 2 && item != null && item.subsubcategories != undefined) {
                        var subsubcategories = item.subsubcategories;
                        for (var x = 0; x < subsubcategories.length; x++) {
                            //Debug.trace("SSC (" + x + ") Does " + subsubcategories[x].nameKey + "==" + route[i] + "?");
                            if (subsubcategories[x].nameKey == route[i]) {
                                item = subsubcategories[x];
                            }
                        }
                    }
                    else if (i == 3 && item != null && item.subsubsubcategories != undefined) {
                        var subsubsubcategories = item.subsubsubcategories;
                        for (var x = 0; x < subsubsubcategories.length; x++) {
                            //Debug.trace("SSSC (" + x + ") Does " + subsubsubcategories[x].nameKey + "==" + route[i] + "?");
                            if (subsubsubcategories[x].nameKey == route[i]) {
                                item = subsubsubcategories[x];
                            }
                        }
                    }


                    if (i != (route.length - 1) && item != null) {
                        if (lastItemPushed.fullyQualifiedName != item.fullyQualifiedName)
                        {
                            vm.breadcrumbs.push({ name: item.name, link: "browse/" + item.fullyQualifiedName })
                            lastItemPushed = item;
                        }
                    }
                    else {
                        if (productName != null && lastAdded != true) {
                            vm.breadcrumbs.push({ name: productName, link: null }); //<--Last item, or a product, no link required
                            lastAdded = true;
                        }
                        else if (item != null) {
                            if (lastItemPushed.fullyQualifiedName != item.fullyQualifiedName) {
                                vm.breadcrumbs.push({ name: item.name, link: null }); //<--Last item, no link required
                                lastItemPushed = item;
                                vm.currentSectionName = item.name //<-- Used for partial view remainder items header
                            }

                        }
                    }

                }
            }

            //if (loadProduct)
            //{
                //vm.getProduct();
            //}



        }



        /* ==========================================

               MENU

        ==========================================*/

        /* ==========================================
               DRAWERS
        ==========================================*/

        vm.categories =
            {
                items: null,
                selectedItem: null,
                path: null,
            }

        vm.category =
           {
               name: null,
               nameKey: null,
               items: null,
               selectedItem: null,
               path: null,
               image: null,
               
               clear: function () {
                   this.selectedItem = null;
                   this.name = null;
                   this.nameKey = null;
                   this.path = null;
                   this.image = null,
                   this.items = null;
               }
           }

        vm.subcategory =
           {
               name: null,
               nameKey: null,
               items: null,
               selectedItem: null,
               path: null,
               image: null,

               clear: function () {
                   this.selectedItem = null;
                   this.name = null;
                   this.nameKey = null;
                   this.path = null;
                   this.image = null,
                   this.items = null;
               }
           }

        vm.subsubcategory =
           {
               name: null,
               nameKey: null,
               items: null,
               selectedItem: null,
               path: null,
               image: null,

               clear: function () {
                   this.selectedItem = null;
                   this.name = null;
                   this.nameKey = null;
                   this.path = null;
                   this.image = null,
                   this.items = null;
               }

           }
        vm.subsubsubcategory =
           {               
               name: null,
               nameKey: null,
               selectedItem: null,
               path: null,
               image: null,

               clear : function()
               {
                   this.selectedItem = null;
                   this.name = null;
                   this.nameKey = null;
                   this.path = null;
                   this.image = null;
               }
           }

        //DAISY CHAIN 2 ---------------------
        vm.getCategories = function()
        {
            categoryServices.getCategoryTree(vm.account.accountNameKey, vm.apiDomain)
            .success(function (data, status, headers, config) {

                //vm.loading = false;
                vm.categories.items = data.categories;
                

                /* ========================================================================
        
                       Determine Categorization Depth on FIRST LOAD and set up menu
        
                ===========================================================================*/
                
                // var menuItem = null;

                vm.pageType = "home";

                if ($routeParams.categoryNameKey) {
                    vm.fullyQualifiedName += $routeParams.categoryNameKey;
                    vm.pageType = "browse";
                    vm.categorizationType = "category";
                    vm.subcategorizationType = "subcategory";
                    vm.nameKey = $routeParams.categoryNameKey;
                    vm.categorizationSearchFilter = "categoryNameKey eq '" + $routeParams.categoryNameKey + "'";

                   

                    //Load category item into backstack and list
                    for (var x = 0; x < vm.categories.items.length; x++) {
                        //Debug.trace("Does " + vm.categories.items[x].nameKey + "==" + route[i] + "?");
                        if (vm.categories.items[x].nameKey == vm.nameKey) {

                            //menuItem = vm.categories.items[x];

                            vm.category.path = vm.fullyQualifiedName;
                            vm.category.items = vm.categories.items[x].subcategories;
                            vm.categories.selectedItem = vm.categories.items[x];
                            vm.category.name = vm.categories.items[x].name;
                            vm.category.image = vm.categories.items[x].images.default.thumbnail.url;
                            vm.category.nameKey = vm.categories.items[x].nameKey;
                        }
                    }
                    
                    if (vm.category.items != null)
                    {
                        if (vm.category.items.length > 0)
                        {
                            $("#slider0").hide();
                            $("#slider1").show();
                        }
                    }


                   // if (vm.category.items.length == 1) {
                        //vm.category.selectedItem = vm.category.items[0];
                   // }

                }
                if ($routeParams.subcategoryNameKey) {
                    vm.fullyQualifiedName += "/" + $routeParams.subcategoryNameKey;
                    vm.pageType = "browse";
                    vm.categorizationType = "subcategory";
                    vm.subcategorizationType = "subsubcategory";
                    vm.nameKey = $routeParams.subcategoryNameKey;
                    vm.categorizationSearchFilter += " and subcategoryNameKey eq '" + $routeParams.subcategoryNameKey + "'";


                    //Load subcategory item into backstack and list
                    if (vm.category.items != null)
                    {
                        for (var x = 0; x < vm.category.items.length; x++) {
                            //Debug.trace("Does " + vm.categories.items[x].nameKey + "==" + route[i] + "?");
                            if (vm.category.items[x].nameKey == vm.nameKey) {

                                // menuItem = vm.categories.items[x];

                                vm.subcategory.path = vm.fullyQualifiedName;
                                vm.subcategory.items = vm.category.items[x].subsubcategories;
                                vm.category.selectedItem = vm.category.items[x];
                                vm.subcategory.name = vm.category.items[x].name;
                                vm.subcategory.image = vm.category.items[x].images.default.thumbnail.url;
                                vm.subcategory.nameKey = vm.category.items[x].nameKey;
                            }
                        }
                    }


                    if (vm.subcategory.items != null) {
                        if (vm.subcategory.items.length > 0) {
                            $("#slider1").hide();
                            $("#slider2").show();
                        }
                    }



                    //if (vm.subcategory.items.length == 1) {
                        //vm.subcategory.selectedItem = vm.subcategory.items[0];
                    //}
                    

                }
                if ($routeParams.subsubcategoryNameKey) {
                    vm.fullyQualifiedName += "/" + $routeParams.subsubcategoryNameKey;
                    vm.pageType = "browse";
                    vm.categorizationType = "subsubcategory";
                    vm.subcategorizationType = "subsubsubcategory";
                    vm.nameKey = $routeParams.subsubcategoryNameKey;
                    vm.categorizationSearchFilter += " and subsubcategoryNameKey eq '" + $routeParams.subsubcategoryNameKey + "'";

                    

                    //Load subsubcategory item into backstack and list
                    if (vm.subcategory.items != null)
                    {
                        for (var x = 0; x < vm.subcategory.items.length; x++) {
                            //Debug.trace("Does " + vm.categories.items[x].nameKey + "==" + route[i] + "?");
                            if (vm.subcategory.items[x].nameKey == vm.nameKey) {

                                // menuItem = vm.categories.items[x];

                                vm.subsubcategory.path = vm.fullyQualifiedName;
                                vm.subsubcategory.items = vm.subcategory.items[x].subsubsubcategories;
                                vm.subcategory.selectedItem = vm.subcategory.items[x];
                                vm.subsubcategory.name = vm.subcategory.items[x].name;
                                vm.subsubcategory.image = vm.subcategory.items[x].images.default.thumbnail.url;
                                vm.subsubcategory.nameKey = vm.subcategory.items[x].nameKey;
                            }
                        }
                    }


                    if (vm.subsubcategory.items != null) {
                        if (vm.subsubcategory.items.length > 0) {
                            $("#slider2").hide();
                            $("#slider3").show();
                        }
                    }


                }
                if ($routeParams.subsubsubcategoryNameKey) {
                    vm.fullyQualifiedName += "/" + $routeParams.subsubsubcategoryNameKey;                    
                    vm.categorizationType = "subsubsubcategory";
                    vm.subcategorizationType = null;
                    vm.nameKey = $routeParams.subsubsubcategoryNameKey;
                    vm.categorizationSearchFilter += " and subsubsubcategoryNameKey eq '" + $routeParams.subsubsubcategoryNameKey + "'";

                    vm.pageType = "browse";

                    //Load subsubcategory item into backstack and list
                    if (vm.subsubcategory.items != null)
                    {
                        for (var x = 0; x < vm.subsubcategory.items.length; x++) {
                            //Debug.trace("Does " + vm.categories.items[x].nameKey + "==" + route[i] + "?");
                            if (vm.subsubcategory.items[x].nameKey == vm.nameKey) {

                                // menuItem = vm.categories.items[x];

                                vm.subsubsubcategory.path = vm.fullyQualifiedName;
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
                if ($routeParams.productNameKey) {
                    vm.fullyQualifiedName += "/" + $routeParams.productNameKey;
                    vm.pageType = "product";
                    vm.nameKey = $routeParams.productNameKey;
                    //vm.productSearchFilter += "and subsubsubcategoryNameKey eq '" + $routeParams.subsubsubcategoryNameKey + "'";
                    //vm.subsubsubcategoryNameKey = $routeParams.subsubsubcategoryNameKey;
                }

                

                /* ========================================================================
                        
                    Load up home, category or product page
                        
                ===========================================================================*/
                

                //Load up properties
                vm.getFeaturedProperties();
                //vm.getDetailProperties();


                if (vm.pageType == 'product') {
                    vm.getProduct();
                }
                else if (vm.pageType == 'home') {
                    vm.searchProductsHome();
                }
                else if (vm.pageType == 'browse') {
                    vm.searchProducts();
                }

                //If this is not a product deep link then create breadcrumbs (product call handles this for us)
                if ($routeParams.productNameKey == undefined) {
                    vm.updateBreadcrumbs(vm.fullyQualifiedName);
                }



                /*

                if (vm.categorizationType != null)
                {
                    //We are in a categorization page:
                    //vm.getCategorization();
                }
                else
                {
                    //We are on the home page, start with featured products
                    
                    vm.filter = "(tags/any(t: t eq 'Featured'))"
                    //vm.getCategorization();
                    vm.searchProducts();
                }
                

                //setTheme();*/

            })
            .error(function (data, status, headers, config) {

            })
        }

        /* ==========================================
               RE-ROUTING & MENU
        ==========================================*/

        //vm.selectedItem = null;


        vm.updateSlideMenu = function (item, slideIndex) {        

            //Debug.trace("Slide index forward: " + slideIndex);

            vm.loadingMobile = true;

            $("#home-body").hide();
            $("#browse-partial").hide();
            $("#browse-full").hide();
            $("#product").hide();

            if (slideIndex == 0) {

                vm.category.path = item.fullyQualifiedName;
                vm.category.name = item.name;
                vm.category.image = item.images.default.thumbnail.url;
                vm.category.nameKey = item.nameKey;
                vm.categories.selectedItem = item;

                vm.categorizationType = "category";

                if (item.subcategories.length > 0)
                {
                    
                    //only initiate slide if there are sub items to display
                    $("#slider0").hide("slide", { direction: "left" }, 400);
                    $timeout(function () {
                        $("#slider1").fadeIn(400);
                        vm.category.items = JSON.parse(JSON.stringify(item.subcategories));
                

                    }, 420)                
                }                
                else
                {
                        //Just bold the item
                }

                //For search call
                //vm.pageType = "browse";

                vm.categorizationSearchFilter = "categoryNameKey eq '" + vm.category.nameKey + "'";

            }
            else if (slideIndex == 1) {

                vm.subcategory.path = item.fullyQualifiedName;
                vm.subcategory.name = item.name;
                vm.subcategory.image = item.images.default.thumbnail.url;
                vm.subcategory.nameKey = item.nameKey;
                vm.category.selectedItem = item;

                vm.categorizationType = "subcategory";

                if (item.subsubcategories.length > 0)
                {                  

                    //only initiate slide if there are sub items to display
                    $("#slider1").hide("slide", { direction: "left" }, 400);
                    $timeout(function () {
                        $("#slider2").fadeIn(400);
                        vm.subcategory.items = JSON.parse(JSON.stringify(item.subsubcategories));

                    }, 420)
               
                }
                else {
                    //Just bold the item
                }

                //For search call
                //vm.pageType = "browse";
                vm.categorizationSearchFilter = "categoryNameKey eq '" + vm.category.nameKey + "'";
                vm.categorizationSearchFilter += " and subcategoryNameKey eq '" + vm.subcategory.nameKey + "'";

            }
            else if (slideIndex == 2) {

                vm.subsubcategory.path = item.fullyQualifiedName;
                vm.subsubcategory.name = item.name;
                vm.subsubcategory.image = item.images.default.thumbnail.url;
                vm.subsubcategory.nameKey = item.nameKey;
                vm.subcategory.selectedItem = item;

                vm.categorizationType = "subsubcategory";

                if (item.subsubsubcategories.length > 0) {                   

                    //only initiate slide if there are sub items to display
                    $("#slider2").hide("slide", { direction: "left" }, 400);
                    $timeout(function () {
                        $("#slider3").fadeIn(400);
                        vm.subsubcategory.items = JSON.parse(JSON.stringify(item.subsubsubcategories));

                    }, 420)

                }
                else {
                    //Just bold the item
                }

                //For search call
                //vm.pageType = "browse";
                vm.categorizationSearchFilter = "categoryNameKey eq '" + vm.category.nameKey + "'";
                vm.categorizationSearchFilter += " and subcategoryNameKey eq '" + vm.subcategory.nameKey + "'";
                vm.categorizationSearchFilter += " and subsubcategoryNameKey eq '" + vm.subsubcategory.nameKey + "'";
            }
            else if (slideIndex == 3) {

                vm.subsubsubcategory.path = item.fullyQualifiedName;
                vm.subsubsubcategory.name = item.name;
                vm.subsubsubcategory.image = item.images.default.thumbnail.url;
                vm.subsubsubcategory.nameKey = item.nameKey;
                vm.subsubcategory.selectedItem = item;

                //Just bold the item
                vm.categorizationType = "subsubsubcategory";

                //For search call
                //vm.pageType = "browse";
                vm.categorizationSearchFilter = "categoryNameKey eq '" + vm.category.nameKey + "'";
                vm.categorizationSearchFilter += " and subcategoryNameKey eq '" + vm.subcategory.nameKey + "'";
                vm.categorizationSearchFilter += " and subsubcategoryNameKey eq '" + vm.subsubcategory.nameKey + "'";
                vm.categorizationSearchFilter += " and subsubsubcategoryNameKey eq '" + vm.subsubsubcategory.nameKey + "'";
            } 

            vm.fullyQualifiedName = item.fullyQualifiedName;

            $location.update_path('/browse/' + item.fullyQualifiedName, true);
            vm.updateBreadcrumbs(item.fullyQualifiedName);

            $timeout(function () {
                vm.searchProducts();
            }, 440)

            //vm.filter = "(fullyQualifiedName eq " + item.fullyQualifiedName + ")"
            
            //vm.getCategorization(); //<-- REMOVE????

        }

        vm.slideMenuBack = function (slideIndex)
        {
            //Debug.trace("Slide index back: " + slideIndex);
            vm.loadingMobile = true;

            $("#home-body").hide();
            $("#browse-partial").hide();
            $("#browse-full").hide();
            $("#product").hide();
            
            if (slideIndex == 1) {

                $("#slider1").fadeOut(200);
                $timeout(function () {
                    //vm.drawerItems = JSON.parse(JSON.stringify(vm.drawerItems0));
                    $("#slider0").show("slide", { direction: "left" }, 400);                    
                }, 240)

                $location.update_path('/', true);
                vm.updateBreadcrumbs(null, null);

                //For search call
                //vm.categorizationSearchFilter = "categoryNameKey eq '" + vm.category.nameKey + "'";

                vm.category.clear();
                vm.subcategory.clear();
                vm.subsubcategory.clear();
                vm.subsubsubcategory.clear();

                vm.searchProductsHome();
            }
            else if (slideIndex == 2) {

                $("#slider2").fadeOut(200);
                $timeout(function () {
                    //vm.drawerItems = JSON.parse(JSON.stringify(vm.drawerItems1));
                    $("#slider1").show("slide", { direction: "left" }, 400);                 
                }, 240)

                $location.update_path('/browse/' + vm.category.path, true);

                //fqn = vm.category.path

                vm.updateBreadcrumbs(vm.category.path);

                //For search call
                vm.categorizationSearchFilter = "categoryNameKey eq '" + vm.category.nameKey + "'";
                //vm.categorizationSearchFilter += " and subcategoryNameKey eq '" + vm.subcategory.nameKey + "'";

                vm.subcategory.clear();
                vm.subsubcategory.clear();
                vm.subsubsubcategory.clear();

                vm.searchProducts();
            }
            else if (slideIndex == 3) {                

                $("#slider3").fadeOut(200);

                $timeout(function () {
                    //vm.drawerItems = JSON.parse(JSON.stringify(vm.drawerItems2));
                    $("#slider2").show("slide", { direction: "left" }, 400);                    
                }, 240)

                $location.update_path('/browse/' + vm.subcategory.path, true);
                vm.updateBreadcrumbs(vm.subcategory.path);

                //fqn = vm.subcategory.path
                
                //For search call
                vm.categorizationSearchFilter = "categoryNameKey eq '" + vm.category.nameKey + "'";
                vm.categorizationSearchFilter += " and subcategoryNameKey eq '" + vm.subcategory.nameKey + "'";
                //vm.categorizationSearchFilter += " and subsubcategoryNameKey eq '" + vm.subsubcategory.nameKey + "'";

                vm.subsubcategory.clear();
                vm.subsubsubcategory.clear();

                vm.searchProducts();
                
            }

           
        }

        vm.loadBreadcrumb = function(route)
        {
            vm.pageType = "home";

            vm.categories.selectedItem = null;
            vm.category.selectedItem = null;
            vm.subcategory.selectedItem = null;
            vm.subsubcategory.selectedItem = null;
            vm.subsubsubcategory.selectedItem = null;


            //Debug.trace(route);
            var routeParams = route.split("/");

            //Debug.trace(routeParams);

            if (routeParams[0] == 'browse')
            {
                if (routeParams[1]) {
                    vm.fullyQualifiedName += routeParams[1];
                    vm.pageType = "browse";
                    vm.categorizationType = "category";
                    vm.subcategorizationType = "subcategory";
                    vm.nameKey = routeParams[1];
                    vm.categorizationSearchFilter = "categoryNameKey eq '" + routeParams[1] + "'";



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

                    if (vm.category.items != null)
                    {
                        if (vm.category.items.length > 0) {
                            $("#slider0").hide();
                            $("#slider1").show();

                            $("#slider2").hide();
                            $("#slider3").hide();
                        }
                        else if (vm.category.items.length == 1) {
                            vm.category.selectedItem = vm.category.items[0];
                        }
                    }


                }
                if (routeParams[2]) {
                    vm.fullyQualifiedName += "/" + routeParams[2];
                    vm.pageType = "browse";
                    vm.categorizationType = "subcategory";
                    vm.subcategorizationType = "subsubcategory";
                    vm.nameKey = routeParams[2];
                    vm.categorizationSearchFilter += " and subcategoryNameKey eq '" + routeParams[2] + "'";


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

                    if (vm.subcategory.items != null)
                    {
                        if (vm.subcategory.items.length > 0) {
                            $("#slider1").hide();
                            $("#slider2").show();

                            $("#slider0").hide();
                            $("#slider3").hide();
                        }
                        else if (vm.subcategory.items.length == 1) {
                            vm.subcategory.selectedItem = vm.subcategory.items[0];
                        }
                    }



                }
                else {
                    vm.subcategory.clear();
                }

                if (routeParams[3]) {
                    vm.fullyQualifiedName += "/" + routeParams[3];
                    vm.pageType = "browse";
                    vm.categorizationType = "subsubcategory";
                    vm.subcategorizationType = "subsubsubcategory";
                    vm.nameKey = routeParams[3];
                    vm.categorizationSearchFilter += " and subsubcategoryNameKey eq '" + routeParams[3] + "'";



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

                            $("#slider0").hide();
                            $("#slider1").hide();

                            if (vm.subsubcategory.items.length == 1)
                             {
                             vm.subsubcategory.selectedItem = vm.subsubcategory.items[0];
                            }
                        }

                    }
                    

                }
                else {
                    vm.subsubcategory.clear();
                }

                if (routeParams[4]) {
                    vm.fullyQualifiedName += "/" + routeParams[4];
                    vm.categorizationType = "subsubsubcategory";
                    vm.subcategorizationType = null;
                    vm.nameKey = routeParams[4];
                    vm.categorizationSearchFilter += " and subsubsubcategoryNameKey eq '" + routeParams[4] + "'";

                    vm.pageType = "browse";

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


                    if (vm.subsubsubcategory.items != null)
                    {
                        if (vm.subsubsubcategory.items.length == 1) {
                         vm.subsubsubcategory.selectedItem = vm.subsubsubcategory.items[0];
                        }
                    }
                    
                }
                else {
                    vm.subsubsubcategory.clear();
                }
            }
            

            if (vm.pageType == 'home') {

                $("#slider0").fadeIn(800);
                $("#slider1").hide();
                $("#slider2").hide();
                $("#slider3").hide();

                vm.category.clear();
                vm.subcategory.clear();
                vm.subsubcategory.clear();
                vm.subsubsubcategory.clear();
            
                vm.searchProductsHome();
                $location.update_path('/', true);
                vm.updateBreadcrumbs(null, null);
            }
            else if (vm.pageType == 'browse') {
                vm.searchProducts();
                $location.update_path(route, true);
                vm.updateBreadcrumbs(route.substring(7, route.length));
            }

        }


        vm.loadProduct = function (path) {

            var pathParams = path.split("/");

            vm.fullyQualifiedName = path;
            vm.pageType = "product";

            vm.nameKey = pathParams[pathParams.length];

            $location.update_path('item/' + path, true);
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







        /* ==========================================
                Theming
        ==========================================*/

        //var base = { 'background-color': 'blue' }

        //vm.theme = {
        //base : base
        //}

        var setTheme = function () {
            document.body.style.background = "#" + vm.account.themeSettings.colors.background;
            document.body.style.foreground = "#" + vm.account.themeSettings.colors.foreground;
            document.title = vm.account.accountName;
        }




        /* ==========================================

               BODY

        ==========================================*/

        //vm.updateBody = function()
        //{

        //}

        /* ==========================================

               END BODY

        ==========================================*/











        /* ==========================================
               GET ACCOUNT SETTINGS
           ==========================================*/

        
        //DAISY CHAIN 1 ---------------------
        vm.getAccountSettings = function () {

            accountServices.getAccount(vm.accountNameKey, vm.apiDomain)
            .success(function (data, status, headers, config) {
                
                vm.account = data.account;
                vm.getCategories();
                vm.getSortables();

                document.title = vm.account.accountName;

                vm.homeOrdering = "dateCreated desc";
                vm.partialOrdering = "dateCreated desc";
                vm.fullOrdering = "dateCreated desc";
                

            })
            .error(function (data, status, headers, config) {

            })
        }

        vm.getSortables = function()
        {
            searchServices.getSortables(vm.account.accountNameKey, vm.apiDomain)
            .success(function (data, status, headers, config) {

                vm.sortables = data.sortables;
                vm.sortables.shift(); //<-- shift removes the first item from the array (relevance)
                vm.fullSelectedSortable = vm.sortables[0];
                
            })
            .error(function (data, status, headers, config) {

            })
        }

        vm.updateSorting = function (sortable) {
            vm.fullSelectedSortable = sortable;
            vm.fullOrdering = sortable.orderByString;
            vm.searchProducts();
        }

        /* ==========================================
               Theming
        ==========================================

        //var base = { 'background-color': 'blue' }

        //vm.theme = {
        //base : base
        //}

        var setTheme = function () {
            document.body.style.background = "#" + vm.account.themeSettings.colors.background;
            document.body.style.foreground = "#" + vm.account.themeSettings.colors.foreground;
            document.title = vm.account.accountName;
        }
       
*/


        /* ==========================================
               SEARCH HOME
           ==========================================*/

        vm.featuredProducts = [];
        vm.homeResults = []

        vm.searchProductsHome = function() {

            vm.loading = true;
            vm.loadingMobile = true;
            vm.pageType = "home";
            vm.featuredProducts = [];
            vm.homeResults = [];
            vm.homeResultsRemaining = [];
            vm.showRemainingHeader = false;

            $("#browse-partial-body").hide();
            $("#browse-full-body").hide();
            $("#home-body").hide();
            $("#product-body").hide();

            var filter = "(tags/any(t: t eq 'Featured'))"

            //Get Featured Items -----
            searchServices.searchProducts(vm.account.accountNameKey, vm.apiDomain, vm.query, filter, vm.homeOrdering, 0, 8)
            .success(function (data, status, headers, config) {

                vm.featuredProducts = data.items;

            })
            .error(function (data, status, headers, config) {

            })

            

            //Create homeResults array in proper order
            for (var i = 0; i < vm.categories.items.length; i++)
            {
                // create a closure to preserve the value of "i"
                (function(i) {
                    var name = vm.categories.items[i].name;
                    var nameKey = vm.categories.items[i].nameKey;
                    vm.homeResults.push({name: name, nameKey: nameKey });
                }(i));
            }

           for (var i = 0; i < vm.homeResults.length; i++)
           {
               // create a closure to preserve the value of "i"
               (function (i) {
                   var filter = "(categoryNameKey eq '" + vm.homeResults[i].nameKey + "')"
                   searchServices.searchProducts(vm.account.accountNameKey, vm.apiDomain, vm.query, filter, vm.homeOrdering, 0, 6)
                  .success(function (data, status, headers, config) {
                      vm.homeResults[i].items = data.items;

                      if (vm.homeResults[i].items.length != null)
                      {
                          if (vm.homeResults[i].items.length > 5)
                          {
                            vm.showRemainingHeader = true;
                          }
                          if (vm.homeResults[i].items.length < 6) {
                              vm.homeResultsRemaining = vm.homeResultsRemaining.concat(vm.homeResults[i].items);
                          }
                      }

                  })
                }(i));
           }

           $timeout(function () {
               vm.loading = false;
               vm.loadingMobile = false;
               $("#home-body").fadeIn(800);
           }, 300);
            

            //Get Top Items From Each Top Category-----
/*
            for (var i = 0; i < vm.categories.items.length; i++)
            {
                // create a closure to preserve the value of "i"
                  (function(i) {

                      var filter = "(categoryNameKey eq '" + vm.categories.items[i].nameKey + "')"
                      var catName = vm.categories.items[i].name;

                      searchServices.searchProducts(vm.account.accountNameKey, vm.apiDomain, vm.query, filter, vm.orderBy, 0, 6)
                      .success(function (data, status, headers, config) {
                          vm.homeResults.push({
                              name: catName, items: data.items
                          });

                      })
                      .error(function (data, status, headers, config) {

                      })

                  }(i));

                  $timeout(function(){}, 5000);

            }*/

        }



        /* =============================================
         * 
         *   PAGINATION
         * 
         =============================================*/

        vm.amountToSkip = 0;
        vm.resultsAvailable = 0;
        vm.amountPerPage = 48;
        vm.searchingMore = false;
        vm.currentProductListFilter = null;

        $(window).scroll(function () {
            if ($(window).scrollTop() + $(window).height() == $(document).height()) {

                if (vm.amountToSkip < vm.resultsAvailable && vm.fullResults != null && vm.searchingMore == false) {

                    vm.searchingMore = true;

                    
                    searchServices.searchProducts(vm.account.accountNameKey, vm.apiDomain, '', vm.currentProductListFilter, vm.fullOrdering, vm.amountToSkip, vm.amountPerPage)
                    .success(function (data, status, headers, config) {

                        //vm.searchQueryCompleted = vm.searchQuery;
                        vm.searchingMore = false;
                        vm.resultsAvailable = data.count;

                        //vm.paginationInfo.lastResultCount = data.Results.length;


                        vm.amountToSkip = vm.amountToSkip + data.items.length;

                        //Append the data
                        vm.fullResults = vm.fullResults.concat(data.items); //= .push(data.Results);// = [].concat(vm.searchResults, data);


                    })
                    .error(function (data, status, headers, config) {

                    })
                }
            }
        });



        /* ==========================================
               SEARCH CATEGORY
           ==========================================*/


        //vm.facets = [];
        //vm.sortables = [];

        //vm.initialLoad = true;
        vm.query = "";
        vm.filter = "";
        //vm.orderBy = "relevance";


        vm.partialResults = []
        vm.fullResults = []

        vm.searchProducts = function() {

            vm.loading = true;
            vm.loadingMobile = true;
            vm.pageType = "browse-full";
            vm.featuredProducts = [];
            vm.partialResults = [];
            vm.partialResultsRemaining = [];
            vm.fullResults = [];

            $("#browse-partial-body").hide();
            $("#browse-full-body").hide();
            $("#home-body").hide();
            $("#product-body").hide();

            var categorizationDepth = "category";
            var _pageType = "browse-full";
            
            //Determin categorization depth, determine pageType & build filter       
            
            
            if (vm.category.nameKey != null)
            {
                var filter = "categoryNameKey eq '" + vm.category.nameKey + "'"

                if (vm.category.items != null && vm.category.items.length > 1)
                {
                    _pageType = "browse-partial";
                }
                else {
                    _pageType = "browse-full";
                }
            }


            if (vm.subcategory.nameKey != null) {
                filter += " and subcategoryNameKey eq '" + vm.subcategory.nameKey + "'"
                categorizationDepth = "subcategory";

                if (vm.subcategory.items != null && vm.subcategory.items.length > 1) {
                    _pageType = "browse-partial";
                }
                else {
                    _pageType = "browse-full";
                }
            }
            

            if (vm.subsubcategory.nameKey != null) {
                filter += " and subsubcategoryNameKey eq '" +vm.subsubcategory.nameKey + "'"
                categorizationDepth = "subsubcategory";

                if (vm.subsubcategory.items != null && vm.subsubcategory.items.length > 1) {
                    _pageType = "browse-partial";
                }
                else {
                    _pageType = "browse-full";
                }
            }
            

            if (vm.subsubsubcategory.nameKey != null) { 
                filter += " and subsubsubcategoryNameKey eq '" + vm.subsubsubcategory.nameKey + "'"
                categorizationDepth = "subsubsubcategory";
                _pageType = "browse-full";
            }

            //Debug.trace("pageType:" + _pageType);
            //Debug.trace("categorizationDepth:" + categorizationDepth);

            vm.pageType = _pageType;

            if(vm.pageType == "browse-partial")
            {
                var subItemsKey = "subcategoryNameKey";
                var subCatsWithFullResults = 0;

                // -- Create array of categorizatons in proper order (if partial) ----
                if (categorizationDepth == "category"){
                    for (var i = 0; i < vm.category.items.length; i++) {
                        // create a closure to preserve the value of "i"
                        (function (i) {
                            var name = vm.category.items[i].name;
                            var nameKey = vm.category.items[i].nameKey;
                            var fullyQualifiedName = vm.category.items[i].fullyQualifiedName;
                            vm.partialResults.push({
                                name: name, nameKey: nameKey, fullyQualifiedName: fullyQualifiedName
                            });
                        }(i));
                    }
                }
                else if (categorizationDepth == "subcategory") {
                    subItemsKey = "subsubcategoryNameKey";
                    for (var i = 0; i < vm.subcategory.items.length; i++) {
                        // create a closure to preserve the value of "i"
                        (function (i) {
                            var name = vm.subcategory.items[i].name;
                            var nameKey = vm.subcategory.items[i].nameKey;
                            var fullyQualifiedName = vm.subcategory.items[i].fullyQualifiedName;
                            vm.partialResults.push({
                                name: name, nameKey: nameKey, fullyQualifiedName: fullyQualifiedName
                            });
                        }(i));
                    }
                }
                else if (categorizationDepth == "subsubcategory") {
                    subItemsKey = "subsubsubcategoryNameKey";
                    for (var i = 0; i < vm.subsubcategory.items.length; i++) {
                        // create a closure to preserve the value of "i"
                        (function (i) {
                            var name = vm.subsubcategory.items[i].name;
                            var nameKey = vm.subsubcategory.items[i].nameKey;
                            var fullyQualifiedName = vm.subsubcategory.items[i].fullyQualifiedName;
                            vm.partialResults.push({
                                name: name, nameKey: nameKey, fullyQualifiedName: fullyQualifiedName
                            });
                            }(i));
                        }
                }
                
                // -- Search for each item in partials list --
                for (var i = 0; i < vm.partialResults.length; i++) {

                    //Debug.trace("i:" + i + " /" + vm.partialResults.length);

                    // create a closure to preserve the value of "i"
                    (function (i) {
                        var innerFilter = filter + " and " + subItemsKey + " eq '" + vm.partialResults[i].nameKey + "'"
                        searchServices.searchProducts(vm.account.accountNameKey, vm.apiDomain, vm.query, innerFilter, vm.partialOrdering, 0, 6)
                       .success(function (data, status, headers, config) {
                           vm.partialResults[i].items = data.items;
                           //Debug.trace("count of '" + vm.partialResults[i].name + "': " + vm.partialResults[i].items.length);
                           if (vm.partialResults[i].items.length > 5)
                           {
                              
                               subCatsWithFullResults++;
                               //Debug.trace("subCatsWithFullResults: " + subCatsWithFullResults);
                           }

                           if (i == vm.partialResults.length - 1) { //<-- runs on the LAST loop

                               //Debug.trace("last");
                               $timeout(function () { //We set a timer to allow for the last IF statement above to fully process.
                                   
                                   //Debug.trace("run");
                                   //Debug.trace("Partial results loop complete. total subCatsWithFullResults: " + subCatsWithFullResults);

                                   //function done() {
                                   //Debug.trace("Test 3 ===== " + vm.partialResults.length + " - " + subCatsWithFullResults);
                                   //}

                                   // Test for partial OK -----------
                                   var partialOK = false;

                                   //Debug.trace("Test" + vm.partialResults.length + " - " + subCatsWithFullResults);

                                   if (vm.partialResults.length > 0 && subCatsWithFullResults > 0) {
                                       partialOK = true;
                                   }

                                   //Debug.trace("sections:" + vm.partialResults.length);
                                   //Debug.trace("amount with full:" + subCatsWithFullResults);

                                   // Show or redo depending on test results -----------
                                   if (partialOK) {
                                       //Debug.trace("(((Partial Results OK)))");
                                       
                                       //Place remaining items into remainder
                                       for (var z = 0; z < vm.partialResults.length; z++)
                                       {
                                           if (vm.partialResults[z].items != null)
                                           {
                                                if (vm.partialResults[z].items.length < 6)
                                                {
                                                    vm.partialResultsRemaining = vm.partialResultsRemaining.concat(vm.partialResults[z].items);
                                                }
                                           }
                                           
                                       }


                                       //Partial results OK to display
                                       vm.loading = false;
                                       vm.loadingMobile = false;
                                       $("#browse-partial-body").fadeIn(800);
                                   }
                                   else {
                                       //Debug.trace("...Not enough content for partial results. Loading full results...");

                                       //--Reset---
                                       vm.amountToSkip = 0;
                                       vm.currentProductListFilter = filter;

                                       searchServices.searchProducts(vm.account.accountNameKey, vm.apiDomain, vm.query, filter, vm.fullOrdering, 0, vm.amountPerPage)
                                       .success(function (data, status, headers, config) {
                                           vm.pageType = "browse-full";
                                           vm.fullResults = data.items;

                                           vm.amountToSkip = vm.amountToSkip + vm.amountPerPage;
                                           vm.resultsAvailable = data.count;

                                           vm.loading = false;
                                           vm.loadingMobile = false;
                                           $("#browse-full-body").fadeIn(800);
                                       })
                                   }


                               }, 140);             
                           }
                       })
                    }(i));

                    
                }
                
            }
            else
            {
                // -- Search for each item in full list --
                //--Reset---
                vm.amountToSkip = 0;
                vm.currentProductListFilter = filter;
                searchServices.searchProducts(vm.account.accountNameKey, vm.apiDomain, vm.query, filter, vm.fullOrdering, 0, vm.amountPerPage)
                .success(function (data, status, headers, config) {
                    vm.fullResults = data.items;
                    vm.amountToSkip = vm.amountToSkip + vm.amountPerPage;
                    vm.resultsAvailable = data.count;
                    vm.loading = false;
                    vm.loadingMobile = false;
                    $("#browse-full-body").fadeIn(800);
                })
            }
            
            /*
            //Debug.trace("Testing, total: " + vm.fullResults.length);
            //if results full and less than 6 fill remainign with null items for twitter bootstrap
            if (vm.fullResults.length == 0 && vm.fullResults.length < 6)
            {
                //Debug.trace("Not enought, total: " + vm.fullResults.length);

                for (var i = vm.fullResults.length; i < 6; i++) {
                    //Debug.trace("loop: " + i);
                    vm.fullResults.push({fullyQualifiedName:null, id:i});
                }
            }
            
            //Debug.trace(vm.fullResults);*/

        }





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


        vm.screenType = "desktop";

        $(window).resize(function () {
            
            vm.updateSize();

        });

        vm.updateSize = function()
        {
            var size = $(window).width()

            if (size < 768) {
                vm.screenType = "mobile";
                //Debug.trace("mobile");
                $("#desktopDiv").hide();
                $("#mobileDiv").show();
            }
            else {
                vm.screenType = "desktop";
                //Debug.trace("desktop");
                $("#desktopDiv").show();
                $("#mobileDiv").hide();
            }
        }

        /* ==========================================
               CONTROLLER ACTIVATION
           ==========================================*/

        activate();

        function activate(){

            vm.accountNameKey = AccountNameKey;
            vm.apiDomain = ApiDomain;
            vm.googleMapsApiKey = JSON.parse(CoreServiceSettings_GoogleMapsApiKey);
            

            vm.getAccountSettings();
            $(".nano").nanoScroller();

            vm.updateSize();
        }

    }

})();

