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
        //var currentApp = CurrentApp;
        string moduleName = entity.QualifiedName.FullName.Split('.')[0];
        var module = currentApp.Root.GetModules().Single(m => m.Name == moduleName);

        using var transaction = currentApp!.StartTransaction("Generate OPR microflows for entity " + entity.Name);

        var createMicroflow = microflowService.CreateMicroflow(currentApp, module, "OPR_" + entity.Name + "_Create");
        foreach (IAttribute attribute in entity.GetAttributes())
        {
            //MicroflowReturnValue? returnValue = null;
            //IMicroflowExpression returnExpression = microflowExpressionService.CreateFromString($"${entity.Name}/{attribute.Name}");
            //var attributeInputParameter;
            DataType? attributeDataType = DataType.Unknown;
            
            if (attribute.Type is IStringAttributeType)
            {
                //returnValue = new MicroflowReturnValue(DataType.String, returnExpression);
                attributeDataType = DataType.String;
                //attributeInputParameter = (attribute.Name, DataType.String);
            }
            else if (attribute.Type is IIntegerAttributeType)
            {
                //returnValue = new MicroflowReturnValue(DataType.Integer, returnExpression);
                attributeDataType = DataType.Integer;
            }
            else if (attribute.Type is IBooleanAttributeType)
            {
                //returnValue = new MicroflowReturnValue(DataType.Boolean, returnExpression);
                attributeDataType = DataType.Boolean;
            }
            else if (attribute.Type is IDecimalAttributeType)
            {
                //returnValue = new MicroflowReturnValue(DataType.Decimal, returnExpression);
                attributeDataType = DataType.Decimal;
            }
            else if (attribute.Type is IDateTimeAttributeType)
            {
                //returnValue = new MicroflowReturnValue(DataType.DateTime, returnExpression);
                attributeDataType = DataType.DateTime;
            }
            else if (attribute.Type is IEnumerationAttributeType)
            {
                IQualifiedName<IEnumeration> enumQualifiedName = ((IEnumerationAttributeType)attribute.Type).Enumeration;
                //returnValue = new MicroflowReturnValue(DataType.Enumeration(enumQualifiedName), returnExpression);
                attributeDataType = DataType.Enumeration(enumQualifiedName);
            }
            var entityInputParameter = (entity.Name, DataType.Object(entity.QualifiedName));
            //var getMicroflow = microflowService.CreateMicroflow(currentApp, module, "OPR_" + entity.Name + "_Get" + attribute.Name,
            //    returnValue, entityInputParameter);
            var attributeInputParameter = (attribute.Name, attributeDataType);
            var setMicroflow = microflowService.CreateMicroflow(currentApp, module, "OPR_" + entity.Name + "_Set" + attribute.Name, null, entityInputParameter, attributeInputParameter);
        }

        transaction.Commit();
    }
}

