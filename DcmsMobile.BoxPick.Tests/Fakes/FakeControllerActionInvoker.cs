using System.Web.Mvc;

namespace DcmsMobile.BoxPick.Tests.Fakes
{
    /// <summary>
    /// Provides access to the invoked action and the result returned by it
    /// </summary>
    public class FakeControllerActionInvoker : ControllerActionInvoker
    {
        private ActionResult _invokedResult;

        /// <summary>
        /// The action result which was invoked
        /// </summary>
        public ActionResult InvokedResult
        {
            get
            {
                return _invokedResult;
            }
        }

        private ActionDescriptor _invokedActionDescriptor;

        public ActionDescriptor InvokedActionDescriptor
        {
            get
            {
                return _invokedActionDescriptor;
            }
        }

        /// <summary>
        /// Don't actually invoke anything
        /// </summary>
        /// <param name="controllerContext"></param>
        /// <param name="actionResult"></param>
        protected override void InvokeActionResult(ControllerContext controllerContext, ActionResult actionResult)
        {
            _invokedResult = actionResult;
            //base.InvokeActionResult(controllerContext, actionResult);
        }

        protected override ActionResult CreateActionResult(ControllerContext controllerContext, ActionDescriptor actionDescriptor, object actionReturnValue)
        {
            _invokedActionDescriptor = actionDescriptor;
            return base.CreateActionResult(controllerContext, actionDescriptor, actionReturnValue);
        }

    }
}
