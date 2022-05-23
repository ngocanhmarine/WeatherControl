using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WeatherControl.Control2
{
    internal class BindingExpressionUncommonField : UncommonField<BindingExpression>
    {
        // Methods
        internal void ClearValue(DependencyObject instance)
        {
            BindingExpression expression = base.GetValue(instance);
            if (expression != null)
            {
                expression.Detach();
            }
            base.ClearValue(instance);
        }

        internal void SetValue(DependencyObject instance, BindingExpression bindingExpr)
        {
            base.SetValue(instance, bindingExpr);
            bindingExpr.Attach(instance);
        }
    }

}
