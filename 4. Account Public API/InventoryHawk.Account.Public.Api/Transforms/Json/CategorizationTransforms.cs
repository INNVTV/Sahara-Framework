using InventoryHawk.Account.Public.Api.ApplicationCategorizationService;
using InventoryHawk.Account.Public.Api.ApplicationImageRecordsService;
using InventoryHawk.Account.Public.Api.Controllers;
using InventoryHawk.Account.Public.Api.Models.Json.Categorization;
using InventoryHawk.Account.Public.Api.Models.Json.Products;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace InventoryHawk.Account.Public.Api.Transforms.Json
{
    public static class CategorizationTransforms
    {
        public static CategoryTreeJson CategoryTree(string accountNameKey, List<CategoryTreeModel> categoryTreeIn, bool includeImages)
        {
            var categoryTreeObjectOut = new CategoryTreeJson();
            categoryTreeObjectOut.categories = new List<CategoryTreeListItemJson>();
            categoryTreeObjectOut.categoryDepth = 0;

            foreach (CategoryTreeModel categoryTreeModel in categoryTreeIn)
            {
                var categoryListItem = new CategoryTreeListItemJson
                {
                    //id = categoryTreeModel.ID.ToString(),
                    name = categoryTreeModel.Name,
                    nameKey = categoryTreeModel.NameKey,
                    fullyQualifiedName = categoryTreeModel.FullyQualifiedName
                };

                if(includeImages)
                {
                    categoryListItem.images = Dynamics.Images.BuildDynamicImagesListForJson(accountNameKey, "category", categoryTreeModel.ID.ToString(), true);
                }

                categoryListItem.subcategories = new List<SubcategoryTreeListItemJson>();

                foreach (SubcategoryTreeModel subcategoryTreeModel in categoryTreeModel.Subcategories)
                {
                    if(categoryTreeObjectOut.categoryDepth < 1)
                    {
                        categoryTreeObjectOut.categoryDepth = 1;
                    }

                    var subcategoryListItem = new SubcategoryTreeListItemJson
                    {
                        //id = subcategoryTreeModel.ID.ToString(),
                        name = subcategoryTreeModel.Name,
                        nameKey = subcategoryTreeModel.NameKey,
                        fullyQualifiedName = subcategoryTreeModel.FullyQualifiedName
                    };

                    if (includeImages)
                    {
                        subcategoryListItem.images = Dynamics.Images.BuildDynamicImagesListForJson(accountNameKey, "subcategory", subcategoryTreeModel.ID.ToString(), true);
                    }

                    subcategoryListItem.subsubcategories = new List<SubsubcategoryTreeListItemJson>();

                    foreach (SubsubcategoryTreeModel subsubcategoryTreeModel in subcategoryTreeModel.Subsubcategories)
                    {
                        if (categoryTreeObjectOut.categoryDepth < 2)
                        {
                            categoryTreeObjectOut.categoryDepth = 2;
                        }

                        var subsubcategoryListItem = new SubsubcategoryTreeListItemJson
                        {
                            //id = subsubcategoryTreeModel.ID.ToString(),
                            name = subsubcategoryTreeModel.Name,
                            nameKey = subsubcategoryTreeModel.NameKey,
                            fullyQualifiedName = subsubcategoryTreeModel.FullyQualifiedName
                        };

                        if (includeImages)
                        {
                            subsubcategoryListItem.images = Dynamics.Images.BuildDynamicImagesListForJson(accountNameKey, "subsubcategory", subsubcategoryTreeModel.ID.ToString(), true);
                        }

                        subsubcategoryListItem.subsubsubcategories = new List<SubsubsubcategoryTreeListItemJson>();

                        foreach (SubsubsubcategoryTreeModel subsubsubcategoryTreeModel in subsubcategoryTreeModel.Subsubsubcategories)
                        {
                            if (categoryTreeObjectOut.categoryDepth < 3)
                            {
                                categoryTreeObjectOut.categoryDepth = 3;
                            }

                            var subsubsubcategoryListItem = new SubsubsubcategoryTreeListItemJson
                            {
                                //id = subsubsubcategoryTreeModel.ID.ToString(),
                                name = subsubsubcategoryTreeModel.Name,
                                nameKey = subsubsubcategoryTreeModel.NameKey,
                                fullyQualifiedName = subsubsubcategoryTreeModel.FullyQualifiedName
                            };

                            if (includeImages)
                            {
                                subsubsubcategoryListItem.images = Dynamics.Images.BuildDynamicImagesListForJson(accountNameKey, "subsubsubcategory", subsubsubcategoryTreeModel.ID.ToString(), true);
                            }

                            subsubcategoryListItem.subsubsubcategories.Add(subsubsubcategoryListItem);

                        }

                        subcategoryListItem.subsubcategories.Add(subsubcategoryListItem);

                    }

                    categoryListItem.subcategories.Add(subcategoryListItem);
                }


                categoryTreeObjectOut.categories.Add(categoryListItem);
            }

            return categoryTreeObjectOut;
        }

        public static CategoriesJson Categories(string accountNameKey, List<CategoryModel> categoriesIn)
        {
            var categoriesObjectOut = new CategoriesJson();
            categoriesObjectOut.categories = new List<CategorizationListItemJson>();           

            foreach (CategoryModel categoryModel in categoriesIn)
            {
                var categoryListItem = new CategorizationListItemJson
                {
                    //id = categoryModel.CategoryID.ToString(),
                    name = categoryModel.CategoryName,
                    nameKey = categoryModel.CategoryNameKey,
                    fullyQualifiedName = categoryModel.FullyQualifiedName
                };


                //Get listing image records for each object from Table Storage
                categoryListItem.images = Dynamics.Images.BuildDynamicImagesListForJson(accountNameKey, "category", categoryModel.CategoryID.ToString(), true);
                

                categoriesObjectOut.categories.Add(categoryListItem);
            }

            

            categoriesObjectOut.count = categoriesObjectOut.categories.Count;

            return categoriesObjectOut;
        }

        public static CategoryDetailsJson Category(string accountNameKey, CategoryModel categoryIn, bool includeItems, bool includeHidden)
        {
            var categoryObjectOut = new CategoryDetailsJson();
            categoryObjectOut.category = new Models.Json.Categorization.CategoryJson();
            categoryObjectOut.category.subcategories = new List<CategorizationListItemJson>();


            #region Build out subcategorization list

            foreach (SubcategoryListModel subcategoryModel in categoryIn.Subcategories)
            {
                var subcategoryListItem = new CategorizationListItemJson
                {
                    //id = subcategoryModel.SubcategoryID.ToString(),
                    name = subcategoryModel.SubcategoryName,
                    nameKey = subcategoryModel.SubcategoryNameKey,
                    fullyQualifiedName = subcategoryModel.FullyQualifiedName
                };

                //Get listing images for each subcategory in the list 
                subcategoryListItem.images = Dynamics.Images.BuildDynamicImagesListForJson(accountNameKey, "subcategory", subcategoryModel.SubcategoryID.ToString(), true);

                categoryObjectOut.category.subcategories.Add(subcategoryListItem);
            }

            #endregion


            #region Build out product list

            if(includeItems && categoryIn.Subcategories.Count == 0)
            {
                var account = Common.GetAccountObject(accountNameKey);

                //Search products
                string filter = "(categoryNameKey eq '" + categoryIn.CategoryNameKey + "')";
                var productResults = DataAccess.Search.SearchProducts(account, null, filter, "orderId asc", 0, 1000, false, null, includeHidden);

                categoryObjectOut.category.items = Dynamics.Products.TransformDynamicProductsListForJson(productResults.Results);
            }

            #endregion

            //categoryObjectOut.count = categoryObjectOut.categories.Count;

            //Get images for this category
            categoryObjectOut.category.images = Dynamics.Images.BuildDynamicImagesListForJson(accountNameKey, "category", categoryIn.CategoryID.ToString(), false);

            //categoryObjectOut.category.id = categoryIn.CategoryID.ToString();
            categoryObjectOut.category.name = categoryIn.CategoryName;
            categoryObjectOut.category.nameKey = categoryIn.CategoryNameKey;
            categoryObjectOut.category.fullyQualifiedName = categoryIn.FullyQualifiedName;
            categoryObjectOut.category.description = categoryIn.Description;
            return categoryObjectOut;
        }

        public static SubcategoryDetailsJson Subcategory(string accountNameKey, SubcategoryModel subcategoryIn, bool includeItems, bool includeHidden)
        {
            var subcategoryObjectOut = new SubcategoryDetailsJson();
            subcategoryObjectOut.subcategory = new Models.Json.Categorization.SubcategoryJson();
            subcategoryObjectOut.subcategory.subsubcategories = new List<CategorizationListItemJson>();

            #region include parent objects

            subcategoryObjectOut.category = new CategorizationParentItemJson
            {
                name = subcategoryIn.Category.CategoryName,
                nameKey = subcategoryIn.Category.CategoryNameKey,
                fullyQualifiedName = subcategoryIn.Category.FullyQualifiedName
            };

            #endregion


            #region Build out subcategorization list

            foreach (SubsubcategoryListModel subsubcategoryModel in subcategoryIn.Subsubcategories)
            {
                var subsubcategoryListItem = new CategorizationListItemJson
                {
                    //id = subsubcategoryModel.SubsubcategoryID.ToString(),
                    name = subsubcategoryModel.SubsubcategoryName,
                    nameKey = subsubcategoryModel.SubsubcategoryNameKey,
                    fullyQualifiedName = subsubcategoryModel.FullyQualifiedName
                };

                //Get listing images for each subcategory in the list 
                subsubcategoryListItem.images = Dynamics.Images.BuildDynamicImagesListForJson(accountNameKey, "subsubcategory", subsubcategoryModel.SubsubcategoryID.ToString(), true);

                subcategoryObjectOut.subcategory.subsubcategories.Add(subsubcategoryListItem);
            }

            #endregion


            #region Build out product list

            if (includeItems && subcategoryIn.Subsubcategories.Count == 0)
            {
                var account = Common.GetAccountObject(accountNameKey);

                //Search products
                string filter = "(locationPath eq '" + subcategoryIn.FullyQualifiedName + "')";
                var productResults = DataAccess.Search.SearchProducts(account, null, filter, "orderId asc", 0, 1000, false, null, includeHidden);

                subcategoryObjectOut.subcategory.items = Dynamics.Products.TransformDynamicProductsListForJson(productResults.Results);
            }

            #endregion

            //categoryObjectOut.count = categoryObjectOut.categories.Count;

            //Get images for this category
            subcategoryObjectOut.subcategory.images = Dynamics.Images.BuildDynamicImagesListForJson(accountNameKey, "subcategory", subcategoryIn.SubcategoryID.ToString(), false);

            //subcategoryObjectOut.subcategory.id = subcategoryIn.SubcategoryID.ToString();
            subcategoryObjectOut.subcategory.name = subcategoryIn.SubcategoryName;
            subcategoryObjectOut.subcategory.nameKey = subcategoryIn.SubcategoryNameKey;
            subcategoryObjectOut.subcategory.fullyQualifiedName = subcategoryIn.FullyQualifiedName;
            subcategoryObjectOut.subcategory.description = subcategoryIn.Description;

            return subcategoryObjectOut;
        }

        public static SubsubcategoryDetailsJson Subsubcategory(string accountNameKey, SubsubcategoryModel subsubcategoryIn, bool includeItems, bool includeHidden)
        {
            var subsubcategoryObjectOut = new SubsubcategoryDetailsJson();
            subsubcategoryObjectOut.subsubcategory = new Models.Json.Categorization.SubsubcategoryJson();
            subsubcategoryObjectOut.subsubcategory.subsubsubcategories = new List<CategorizationListItemJson>();

            #region include parent objects

            subsubcategoryObjectOut.category = new CategorizationParentItemJson
            {
                name = subsubcategoryIn.Category.CategoryName,
                nameKey = subsubcategoryIn.Category.CategoryNameKey,
                fullyQualifiedName = subsubcategoryIn.Category.FullyQualifiedName
            };

            subsubcategoryObjectOut.subcategory = new CategorizationParentItemJson
            {
                name = subsubcategoryIn.Subcategory.SubcategoryName,
                nameKey = subsubcategoryIn.Subcategory.SubcategoryNameKey,
                fullyQualifiedName = subsubcategoryIn.Subcategory.FullyQualifiedName
            };

            #endregion

            #region Build out subsubsubcategorization list

            foreach (SubsubsubcategoryListModel subsubsubcategoryModel in subsubcategoryIn.Subsubsubcategories)
            {
                var subsubsubcategoryListItem = new CategorizationListItemJson
                {
                    //id = subsubsubcategoryModel.SubsubcategoryID.ToString(),
                    name = subsubsubcategoryModel.SubsubsubcategoryName,
                    nameKey = subsubsubcategoryModel.SubsubsubcategoryNameKey,
                    fullyQualifiedName = subsubsubcategoryModel.FullyQualifiedName
                };

                //Get listing images for each subcategory in the list 
                subsubsubcategoryListItem.images = Dynamics.Images.BuildDynamicImagesListForJson(accountNameKey, "subsubsubcategory", subsubsubcategoryModel.SubsubsubcategoryID.ToString(), true);

                subsubcategoryObjectOut.subsubcategory.subsubsubcategories.Add(subsubsubcategoryListItem);
            }

            #endregion


            #region Build out product list

            if (includeItems && subsubcategoryIn.Subsubsubcategories.Count == 0)
            {
                var account = Common.GetAccountObject(accountNameKey);

                //Search products
                string filter = "(locationPath eq '" + subsubcategoryIn.FullyQualifiedName + "')";
                var productResults = DataAccess.Search.SearchProducts(account, null, filter, "orderId asc", 0, 1000, false, null, includeHidden);

                subsubcategoryObjectOut.subsubcategory.items = Dynamics.Products.TransformDynamicProductsListForJson(productResults.Results);
            }

            #endregion

            //categoryObjectOut.count = categoryObjectOut.categories.Count;

            //Get images for this category
            subsubcategoryObjectOut.subsubcategory.images = Dynamics.Images.BuildDynamicImagesListForJson(accountNameKey, "subsubcategory", subsubcategoryIn.SubsubcategoryID.ToString(), false);

            //subcategoryObjectOut.subcategory.id = subcategoryIn.SubcategoryID.ToString();
            subsubcategoryObjectOut.subsubcategory.name = subsubcategoryIn.SubsubcategoryName;
            subsubcategoryObjectOut.subsubcategory.nameKey = subsubcategoryIn.SubsubcategoryNameKey;
            subsubcategoryObjectOut.subsubcategory.fullyQualifiedName = subsubcategoryIn.FullyQualifiedName;
            subsubcategoryObjectOut.subsubcategory.description = subsubcategoryIn.Description;

            return subsubcategoryObjectOut;
        }

        public static SubsubsubcategoryDetailsJson Subsubsubcategory(string accountNameKey, SubsubsubcategoryModel subsubsubcategoryIn, bool includeItems, bool includeHidden)
        {
            var subsubsubcategoryObjectOut = new SubsubsubcategoryDetailsJson();
            subsubsubcategoryObjectOut.subsubsubcategory = new Models.Json.Categorization.SubsubsubcategoryJson();

            #region include parent objects

            subsubsubcategoryObjectOut.category = new CategorizationParentItemJson
            {
                name = subsubsubcategoryIn.Category.CategoryName,
                nameKey = subsubsubcategoryIn.Category.CategoryNameKey,
                fullyQualifiedName = subsubsubcategoryIn.Category.FullyQualifiedName
            };

            subsubsubcategoryObjectOut.subcategory = new CategorizationParentItemJson
            {
                name = subsubsubcategoryIn.Subcategory.SubcategoryName,
                nameKey = subsubsubcategoryIn.Subcategory.SubcategoryNameKey,
                fullyQualifiedName = subsubsubcategoryIn.Subcategory.FullyQualifiedName
            };

            subsubsubcategoryObjectOut.subsubcategory = new CategorizationParentItemJson
            {
                name = subsubsubcategoryIn.Subsubcategory.SubsubcategoryName,
                nameKey = subsubsubcategoryIn.Subsubcategory.SubsubcategoryNameKey,
                fullyQualifiedName = subsubsubcategoryIn.Subsubcategory.FullyQualifiedName
            };

            #endregion

            #region Build out product list

            if (includeItems)
            {
                var account = Common.GetAccountObject(accountNameKey);

                //Search products
                string filter = "(locationPath eq '" + subsubsubcategoryIn.FullyQualifiedName + "')";
                var productResults = DataAccess.Search.SearchProducts(account, null, filter, "orderId asc", 0, 1000, false, null, includeHidden);

                subsubsubcategoryObjectOut.subsubsubcategory.items = Dynamics.Products.TransformDynamicProductsListForJson(productResults.Results);
            }

            #endregion

            //categoryObjectOut.count = categoryObjectOut.categories.Count;

            //Get images for this category
            subsubsubcategoryObjectOut.subsubsubcategory.images = Dynamics.Images.BuildDynamicImagesListForJson(accountNameKey, "subsubsubcategory", subsubsubcategoryIn.SubsubsubcategoryID.ToString(), false);

            //subcategoryObjectOut.subcategory.id = subcategoryIn.SubcategoryID.ToString();
            subsubsubcategoryObjectOut.subsubsubcategory.name = subsubsubcategoryIn.SubsubsubcategoryName;
            subsubsubcategoryObjectOut.subsubsubcategory.nameKey = subsubsubcategoryIn.SubsubsubcategoryNameKey;
            subsubsubcategoryObjectOut.subsubsubcategory.fullyQualifiedName = subsubsubcategoryIn.FullyQualifiedName;
            subsubsubcategoryObjectOut.subsubsubcategory.description = subsubsubcategoryIn.Description;

            return subsubsubcategoryObjectOut;
        }

    }
}