
-- Categorization Contraints --

ALTER TABLE [#schema#].[Subcategory]
ADD CONSTRAINT [#schema#_FK_SubcategoryToCategoryID] FOREIGN KEY ([CategoryID])
REFERENCES [#schema#].[Category] ([CategoryID])
GO

ALTER TABLE [#schema#].[Subsubcategory]
ADD CONSTRAINT [#schema#_FK_SubsubcategoryToSubcategoryID] FOREIGN KEY ([SubcategoryID])
REFERENCES [#schema#].[Subcategory] ([SubcategoryID])
GO

ALTER TABLE [#schema#].[Subsubsubcategory]
ADD CONSTRAINT [#schema#_FK_SubsubsubcategoryToSubsubcategoryID] FOREIGN KEY ([SubsubcategoryID])
REFERENCES [#schema#].[Subsubcategory] ([SubsubcategoryID])
GO

-- Property Contraints --
--ALTER TABLE [#schema#].[Property]
--ADD CONSTRAINT [#schema#_FK_PropertyToPropertyTypeID] FOREIGN KEY ([PropertyTypeID])
--REFERENCES [#schema#].[PropertyType] ([PropertyTypeID])
--GO

ALTER TABLE [#schema#].[PropertyValue]
ADD CONSTRAINT [#schema#_FK_PropertyValueToPropertyID] FOREIGN KEY ([PropertyID])
REFERENCES [#schema#].[Property] ([PropertyID])
GO

-- Image Contraints --
--ALTER TABLE [#schema#].[ImageFormat]
--ADD CONSTRAINT [#schema#_FK_ImageFormatToImageGroupID] FOREIGN KEY ([ImageGroupID])
--REFERENCES [#schema#].[ImageGroup] ([ImageGroupID])
--GO

--ALTER TABLE [#schema#].[ImageGroup]
--ADD CONSTRAINT [#schema#_FK_ImageGroupToImageGroupTypeID] FOREIGN KEY ([ImageGroupTypeID])
--REFERENCES [#schema#].[ImageGroupType] ([ImageGroupTypeID])
--GO