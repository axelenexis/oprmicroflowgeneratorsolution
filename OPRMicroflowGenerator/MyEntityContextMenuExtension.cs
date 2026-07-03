// By Axel Brink and Pascal Bos 2026

using Mendix.StudioPro.ExtensionsAPI.Model.DomainModels;
using Mendix.StudioPro.ExtensionsAPI.UI.Menu;
using System.ComponentModel.Composition;
 
namespace OPRMicroflowGenerator;

[method: ImportingConstructor]
[Export(typeof(ContextMenuExtension<>))]
class MyEntityContextMenuExtension(OPRMicroflowGenerator microflowGenerator) : ContextMenuExtension<IEntity>
{
    public override IEnumerable<MenuViewModel> GetContextMenus(IEntity entity)
    {
        yield return new MenuViewModel("Generate OPR microflows", () =>
        {
            if (CurrentApp == null)
                return;

            microflowGenerator.GenerateOPRMicroflows(CurrentApp, entity);
        });

    }


}
