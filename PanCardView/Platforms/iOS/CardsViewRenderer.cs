﻿using Foundation;
using PanCardView;
using PanCardView.iOS;
using UIKit;
using PanCardView.Enums;
using System.ComponentModel;
using static System.Math;


using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Controls.Compatibility.Platform.iOS;

namespace PanCardView.iOS
{
#pragma warning disable
    public class CardsViewRenderer : VisualElementRenderer<CardsView>
#pragma warning restore
    {
        private UISwipeGestureRecognizer _leftSwipeGesture;
        private UISwipeGestureRecognizer _rightSwipeGesture;
        private UISwipeGestureRecognizer _upSwipeGesture;
        private UISwipeGestureRecognizer _downSwipeGesture;

        public CardsViewRenderer()
        {
            _leftSwipeGesture = new UISwipeGestureRecognizer(OnSwiped)
            {
                Direction = UISwipeGestureRecognizerDirection.Left
            };
            _rightSwipeGesture = new UISwipeGestureRecognizer(OnSwiped)
            {
                Direction = UISwipeGestureRecognizerDirection.Right
            };
            _upSwipeGesture = new UISwipeGestureRecognizer(OnSwiped)
            {
                Direction = UISwipeGestureRecognizerDirection.Up
            };
            _downSwipeGesture = new UISwipeGestureRecognizer(OnSwiped)
            {
                Direction = UISwipeGestureRecognizerDirection.Down
            };
        }

        public override void AddGestureRecognizer(UIGestureRecognizer gestureRecognizer)
        {
            base.AddGestureRecognizer(gestureRecognizer);

            if (gestureRecognizer is UIPanGestureRecognizer panGestureRecognizer)
            {
                gestureRecognizer.ShouldRecognizeSimultaneously = ShouldRecognizeSimultaneously;
                gestureRecognizer.ShouldBegin = ShouldBegin;
            }
        }

        protected override void OnElementChanged(ElementChangedEventArgs<CardsView> e)
        {
            base.OnElementChanged(e);
            if (e.OldElement != null)
            {
                e.OldElement.AccessibilityChangeRequested -= OnAccessibilityChangeRequested;
            }

            if (e.NewElement != null)
            {
                SetSwipeGestures();
                Element.AccessibilityChangeRequested += OnAccessibilityChangeRequested;
            }
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);
            if (e.PropertyName == CardsView.IsVerticalSwipeEnabledProperty.PropertyName
                || e.PropertyName == CardsView.IsUserInteractionEnabledProperty.PropertyName)
            {
                SetSwipeGestures();
                return;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _leftSwipeGesture?.Dispose();
                _rightSwipeGesture?.Dispose();
                _upSwipeGesture?.Dispose();
                _downSwipeGesture?.Dispose();
                _leftSwipeGesture = null;
                _rightSwipeGesture = null;
                _upSwipeGesture = null;
                _downSwipeGesture = null;
            }
            base.Dispose(disposing);
        }

        protected virtual void ResetSwipeGestureRecognizer(UISwipeGestureRecognizer swipeGestureRecognizer, bool isForceRemove = false)
        {
            RemoveGestureRecognizer(swipeGestureRecognizer);
            if (isForceRemove)
            {
                return;
            }
            AddGestureRecognizer(swipeGestureRecognizer);
        }

        protected void SetSwipeGestures()
        {
            var shouldRemoveAllSwipes = !(Element?.IsUserInteractionEnabled ?? false);
            var shouldRemoveVerticalSwipes = !(Element?.IsVerticalSwipeEnabled ?? false);

            ResetSwipeGestureRecognizer(_leftSwipeGesture, shouldRemoveAllSwipes);
            ResetSwipeGestureRecognizer(_rightSwipeGesture, shouldRemoveAllSwipes);
            ResetSwipeGestureRecognizer(_upSwipeGesture, shouldRemoveAllSwipes || shouldRemoveVerticalSwipes);
            ResetSwipeGestureRecognizer(_downSwipeGesture, shouldRemoveAllSwipes || shouldRemoveVerticalSwipes);
        }

        private void OnAccessibilityChangeRequested(object sender, bool isEnabled)
        {
            if (sender is View view)
            {
#pragma warning disable
                var nativeView = Microsoft.Maui.Controls.Compatibility.Platform.iOS.Platform.GetRenderer(view)?.NativeView;
#pragma warning restore
                if (nativeView != null)
                {
                    nativeView.AccessibilityElementsHidden = !isEnabled;
                }
            }
        }

        private void OnSwiped(UISwipeGestureRecognizer gesture)
        {
            var swipeDirection = gesture.Direction == UISwipeGestureRecognizerDirection.Left
                ? ItemSwipeDirection.Left
                : gesture.Direction == UISwipeGestureRecognizerDirection.Right
                    ? ItemSwipeDirection.Right
                    : gesture.Direction == UISwipeGestureRecognizerDirection.Up
                        ? ItemSwipeDirection.Up
                        : ItemSwipeDirection.Down;

            Element?.OnSwiped(swipeDirection);
        }

        private bool ShouldBegin(UIGestureRecognizer recognizer)
        {
            if (recognizer is UIPanGestureRecognizer pangesture)
            {
                var superview = pangesture.View.Superview;
                while (superview != null)
                {
                    if (superview is UIScrollView)
                    {
                        var velocity = pangesture.VelocityInView(this);
                        var absVelocityX = Abs(velocity.X);
                        var absVelocityY = Abs(velocity.Y);
                        var isHorizontal = Element.IsHorizontalOrientation;
                        return (absVelocityY < absVelocityX && isHorizontal) ||
                               (absVelocityY > absVelocityX && !isHorizontal);
                    }
                    superview = superview.Superview;
                }
            }

            return true;
        }

        private bool ShouldRecognizeSimultaneously(UIGestureRecognizer gestureRecognizer, UIGestureRecognizer otherGestureRecognizer)
            => Element == null || !(otherGestureRecognizer is UIPanGestureRecognizer) || otherGestureRecognizer.View is CardsViewRenderer;
    }
}