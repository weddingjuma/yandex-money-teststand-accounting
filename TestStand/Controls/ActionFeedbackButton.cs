using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace TestStand.Controls
{
    /// <summary>
    /// Состояние кнопки
    /// </summary>
    public enum ActionFeedbackButtonState
    {
        /// <summary>
        /// Обычное
        /// </summary>
        Normal,
        /// <summary>
        /// Выполнение действия
        /// </summary>
        InProgress,
        /// <summary>
        /// Успех
        /// </summary>
        Success,
        /// <summary>
        /// Ошибка
        /// </summary>
        Error
    }

    /// <summary>
    /// Кнопка, визуально отображающая результат выполнения действия.
    /// </summary>
    public class ActionFeedbackButton : Button
    {
        // Using a DependencyProperty as the backing store for State.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty StateProperty =
            DependencyProperty.Register("State", typeof(ActionFeedbackButtonState), typeof(ActionFeedbackButton), new PropertyMetadata(ActionFeedbackButtonState.Normal, (d, e) =>
            {
                var control = (ActionFeedbackButton)d;
                var newState = (ActionFeedbackButtonState)e.NewValue;
                switch (newState)
                {
                    case ActionFeedbackButtonState.Normal:
                        VisualStateManager.GoToState(control, "Default", true);
                        break;
                    case ActionFeedbackButtonState.InProgress:
                        VisualStateManager.GoToState(control, "InProgress", true);
                        break;
                    case ActionFeedbackButtonState.Success:
                        VisualStateManager.GoToState(control, "Success", true);
                        break;
                    case ActionFeedbackButtonState.Error:
                        VisualStateManager.GoToState(control, "Error", true);
                        break;
                }
            }));

        /// <summary>
        /// Состояние кнопки
        /// </summary>
        public ActionFeedbackButtonState State
        {
            get { return (ActionFeedbackButtonState)GetValue(StateProperty); }
            set { SetValue(StateProperty, value); }
        }
    }
}
