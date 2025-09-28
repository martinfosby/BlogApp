using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;

public abstract class ControllerTestBase<TController> where TController : Controller
{
    protected void InitializeTempData(TController controller)
    {
        var tempDataProvider = Mock.Of<ITempDataProvider>();
        controller.TempData = new TempDataDictionary(new DefaultHttpContext(), tempDataProvider);
    }
}