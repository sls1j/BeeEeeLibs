using BeeEeeLibs.DependencyInjection;
using IocContainterTests.Objs;
using IocContainterTests.Ops;
using Newtonsoft.Json;

var container = new IocContainer();

Action<IocProvider, object> initOpBase = (provider, service) =>
 {
     OpBase opBase = (OpBase)service;
     opBase.SetValue("Value");
 };

container.AddServicesInjectConstructorByBaseType<OpBase>(ServiceLife.Single, initOpBase);
container.AddServiceInjectFields<FieldObj>();
container.AddServiceInjectProperties<PropObj>();

Action firstAction = () => { };
container.AddFunction(nameof(firstAction), firstAction);

string stringValue = "value1";
container.AddValue(nameof(stringValue), stringValue);

int intValue = 123;
container.AddValue(nameof(intValue), intValue);

container.AddExecutorByName("MakeThing");

container.AddServiceInjectConstructor<ErrorObj>();

using (var provider = container.BuildProvider())
{
    var opOne = provider.GetService<OpOne>();
    var opTwo = provider.GetService<OpTwo>();
    var ops = provider.GetServicesByBase<OpBase>();
    var objs = provider.GetServicesByBase<ObjBase>();
    var results = provider.ExecuteAll(x => x.KeyFunction == "MakeThing");
    try
    {
        var errorObj = provider.GetService<ErrorObj>();
    }
    catch (Exception)
    {
        Console.WriteLine("Getting ErrorObj failed like it was suppose to.");
    }
}
