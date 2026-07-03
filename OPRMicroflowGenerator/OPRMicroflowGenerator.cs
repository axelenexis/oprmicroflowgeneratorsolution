using Mendix.StudioPro.ExtensionsAPI.Model;
using Mendix.StudioPro.ExtensionsAPI.Model.DomainModels;
using Mendix.StudioPro.ExtensionsAPI.Model.Enumerations;
using Mendix.StudioPro.ExtensionsAPI.Model.MicroflowExpressions;
using Mendix.StudioPro.ExtensionsAPI.Model.Microflows;
using Mendix.StudioPro.ExtensionsAPI.Model.DataTypes;
using Mendix.StudioPro.ExtensionsAPI.Services;
using System.ComponentModel.Composition;

namespace OPRMicroflowGenerator;

[Export(typeof(OPRMicroflowGenerator))]
[method: ImportingConstructor]
class OPRMicroflowGenerator(IMicroflowService microflowService, IMicroflowExpressionService microflowExpressionService)
{
    public void GenerateOPRMicroflows(IModel currentApp, IEntity entity)
    {
        string moduleName = entity.QualifiedName.FullName.Split('.')[0];
        var module = currentApp.Root.GetModules().Single(m => m.Name == moduleName);

        using var transaction = currentApp!.StartTransaction("Generate OPR microflows for entity " + entity.Name);

        var createMicroflow = microflowService.CreateMicroflow(currentApp, module, "OPR_" + entity.Name + "_Create");
        foreach (IAttribute attribute in entity.GetAttributes())
        {
            DataType? attributeDataType = DataType.Unknown;
            
            if (attribute.Type is IStringAttributeType)
            {
                attributeDataType = DataType.String;
            }
            else if (attribute.Type is IIntegerAttributeType)
            {
                attributeDataType = DataType.Integer;
            }
            else if (attribute.Type is IBooleanAttributeType)
            {
                attributeDataType = DataType.Boolean;
            }
            else if (attribute.Type is IDecimalAttributeType)
            {
                attributeDataType = DataType.Decimal;
            }
            else if (attribute.Type is IDateTimeAttributeType)
            {
                attributeDataType = DataType.DateTime;
            }
            else if (attribute.Type is IEnumerationAttributeType)
            {
                IQualifiedName<IEnumeration> enumQualifiedName = ((IEnumerationAttributeType)attribute.Type).Enumeration;
                attributeDataType = DataType.Enumeration(enumQualifiedName);
            }
            var entityInputParameter = (entity.Name, DataType.Object(entity.QualifiedName));
            var attributeInputParameter = (attribute.Name, attributeDataType);
            var setMicroflow = microflowService.CreateMicroflow(currentApp, module, "OPR_" + entity.Name + "_Set" + attribute.Name, null, entityInputParameter, attributeInputParameter);
        }

        transaction.Commit();
    }
}

