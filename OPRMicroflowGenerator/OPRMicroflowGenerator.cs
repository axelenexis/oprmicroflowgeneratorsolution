using Mendix.StudioPro.ExtensionsAPI.Model;
using Mendix.StudioPro.ExtensionsAPI.Model.DomainModels;
using Mendix.StudioPro.ExtensionsAPI.Model.DataTypes;
using Mendix.StudioPro.ExtensionsAPI.Services;
using System.ComponentModel.Composition;
using Mendix.StudioPro.ExtensionsAPI.Model.Microflows.Actions;

namespace OPRMicroflowGenerator;

[Export(typeof(OPRMicroflowGenerator))]
[method: ImportingConstructor]
class OPRMicroflowGenerator(
    IMicroflowService microflowService, 
    IMicroflowActivitiesService activitiesService, 
    IMicroflowExpressionService expressionService,
    ILogService logService)
{
    public void GenerateOPRMicroflows(IModel currentApp, IEntity entity)
    {
        string moduleName = entity.QualifiedName.FullName.Split('.')[0];
        var module = currentApp.Root.GetModules().Single(m => m.Name == moduleName);

        using var transaction = currentApp!.StartTransaction("Generate OPR microflows for entity " + entity.Name);

        var createMicroflow = microflowService.CreateMicroflow(currentApp, module, "OPR_" + entity.Name + "_Create");
        foreach (IAttribute attribute in entity.GetAttributes())
        {
            var attributeDataType = DetermineDataType(attribute);
            var entityInputParameter = (entity.Name, DataType.Object(entity.QualifiedName));
            var attributeInputParameter = (attribute.Name, attributeDataType);
            var setMicroflow = microflowService.CreateMicroflow(currentApp, module, GetOPRMicroflowName(entity.Name, attribute.Name), null, entityInputParameter, attributeInputParameter);
            var setExpressionString = $"${attributeInputParameter.Name}";
            var setExpression = expressionService.CreateFromString(setExpressionString);
            var activity = activitiesService
                .CreateChangeAttributeActivity(
                    currentApp, 
                    attribute, 
                    ChangeActionItemType.Set, 
                    setExpression, 
                    entity.Name, 
                    CommitEnum.No);
            microflowService.TryInsertAfterStart(setMicroflow, [ activity ]);
        }

        transaction.Commit();
    }

    static string GetOPRMicroflowName(string entityName, string attributeName) 
        => $"OPR_{entityName}_{attributeName}";

    static DataType DetermineDataType(IAttribute attribute)
    {
        DataType? dataType = attribute.Type switch
        {
            IStringAttributeType => DataType.String,
            IIntegerAttributeType => DataType.Integer,
            IBooleanAttributeType => DataType.Boolean,
            IDecimalAttributeType => DataType.Decimal,
            IDateTimeAttributeType => DataType.DateTime,
            IEnumerationAttributeType => GetEnumDataType(attribute),
            _ => DataType.Unknown
        };

        if (dataType is null)
        {
            throw new Exception("Could not determine data type for attribute: " + attribute.Name);
        }

        return dataType;

    }

    static EnumerationType? GetEnumDataType(IAttribute attribute)
    {
        if (attribute is IEnumerationAttributeType)
        {
            var enumQualifiedName = (attribute.Type as IEnumerationAttributeType)?.Enumeration;
            return enumQualifiedName is not null 
                ? DataType.Enumeration(enumQualifiedName) 
                : null;
        }

        return null;
    }
}

