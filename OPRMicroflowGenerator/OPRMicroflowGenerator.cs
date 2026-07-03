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
            MicroflowReturnValue? returnValue = null;
            IMicroflowExpression returnExpression = microflowExpressionService.CreateFromString($"${entity.Name}/{attribute.Name}");

            if (attribute.Type is IStringAttributeType)
            {
                returnValue = new MicroflowReturnValue(DataType.String, returnExpression);
            }
            else if (attribute.Type is IIntegerAttributeType)
            {
                returnValue = new MicroflowReturnValue(DataType.Integer, returnExpression);
            }
            else if (attribute.Type is IBooleanAttributeType)
            {
                returnValue = new MicroflowReturnValue(DataType.Boolean, returnExpression);
            }
            else if (attribute.Type is IDecimalAttributeType)
            {
                returnValue = new MicroflowReturnValue(DataType.Decimal, returnExpression);
            }
            else if (attribute.Type is IDateTimeAttributeType)
            {
                returnValue = new MicroflowReturnValue(DataType.DateTime, returnExpression);
            }
            else if (attribute.Type is IEnumerationAttributeType)
            {
                IQualifiedName<IEnumeration> enumQualifiedName = ((IEnumerationAttributeType)attribute.Type).Enumeration;
                returnValue = new MicroflowReturnValue(DataType.Enumeration(enumQualifiedName), returnExpression);
            }
            var inputParameter = (entity.Name, DataType.Object(entity.QualifiedName));
            var getMicroflow = microflowService.CreateMicroflow(currentApp, module, "OPR_" + entity.Name + "_Get" + attribute.Name,
                returnValue, inputParameter);
            var setMicroflow = microflowService.CreateMicroflow(currentApp, module, "OPR_" + entity.Name + "_Set" + attribute.Name, null, inputParameter);
        }

        transaction.Commit();
    }
}

