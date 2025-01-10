using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SolomonChat.Extensions
{
    /// <summary>
    /// Extension methods for type <see cref="DependencyObject"/>.
    /// </summary>
    public static class DependencyObjectExtensions
    {
        /// <summary>
        /// Verifies that a dependency object is permitted to be accessed from the current thread.
        /// </summary>
        /// <param name="dependencyObject">
        /// A dependency object.
        /// </param>
        /// <exception cref="InvalidOperationException">
        /// Thrown when <paramref name="dependencyObject"/> is not permitted to be accessed from the current
        /// thread.
        /// </exception>
        public static void ThrowExceptionIfNotOnDispatcherThread(this DependencyObject dependencyObject)
        {
            if (dependencyObject != null && !dependencyObject.Dispatcher.HasThreadAccess)
            {
                throw new InvalidOperationException(
                    $"A {dependencyObject.GetType()} object may only be accessed from the UI thread.");
            }
        }

        /// <summary>
        /// Enumerates all ancestors of an object in the visual tree, in order from nearest (i.e.: the parent)
        /// to farthest (i.e.: the root of the tree).
        /// </summary>
        /// <param name="dependencyObject">
        /// A dependency object.
        /// </param>
        /// <returns>
        /// Returns all visual tree ancestors of <paramref name="dependencyObject"/>.
        /// </returns>
        public static IEnumerable<DependencyObject> GetVisualAncestors(this DependencyObject dependencyObject)
        {
            if (dependencyObject == null)
                throw new ArgumentNullException(nameof(dependencyObject));

            DependencyObject currentObject = dependencyObject;

            while (TryFindVisualAncestor(currentObject, out DependencyObject ancestor))
            {
                yield return ancestor;
                currentObject = ancestor;
            }
        }

        public static string GetVisualAncestorsAsString(this DependencyObject dependencyObject)
        {
            IEnumerable<DependencyObject> visualAncestors = dependencyObject.GetVisualAncestors();
            IEnumerable<string> names = visualAncestors.Select(x => x.GetType().Name);

            return string.Join('.', names);
        }

        /// <summary>
        /// Searches the visual tree for a dependency object's closest ancestor of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">
        /// Type of ancestor to find.
        /// </typeparam>
        /// <param name="dependencyObject">
        /// A dependency object.
        /// </param>
        /// <returns>
        /// Returns the closest ancestor of type <typeparamref name="T"/>.
        /// </returns>
        /// <exception cref="Exception">
        /// Thrown when no ancestor of type <typeparamref name="T"/> is found.
        /// </exception>
        public static T FindVisualAncestor<T>(this DependencyObject dependencyObject) where T : DependencyObject
        {
            if (TryFindVisualAncestor(dependencyObject, out T ancestor))
                return ancestor;

            throw new Exception($"No ancestor found of type '{nameof(T)}' = '{typeof(T)}'.");
        }

        /// <summary>
        /// Searches the visual tree for a dependency object's closest ancestor of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">
        /// Type of ancestor to find.
        /// </typeparam>
        /// <param name="dependencyObject">
        /// A dependency object.
        /// </param>
        /// <param name="ancestor">
        /// Returns the closest ancestor of type <typeparamref name="T"/>.  Returns <c>null</c> if not found.
        /// </param>
        /// <returns>
        /// Returns <c>true</c> if and only if an ancestor of type <typeparamref name="T"/> is found.
        /// </returns>
        public static bool TryFindVisualAncestor<T>(this DependencyObject dependencyObject, out T ancestor)
            where T : DependencyObject
        {
            if (dependencyObject == null)
                throw new ArgumentNullException(nameof(dependencyObject));

            DependencyObject parent = VisualTreeHelper.GetParent(dependencyObject);

            if (parent == null)
            {
                ancestor = null;
                return false;
            }

            if (parent is T)
            {
                ancestor = (T)parent;
                return true;
            }

            return TryFindVisualAncestor(parent, out ancestor);
        }

        /// <summary>
        /// Searches the visual tree for a dependency object's closest ancestor of type <typeparamref name="T"/>
        /// for which a specified predicate is <c>true</c>.
        /// </summary>
        /// <typeparam name="T">
        /// Type of ancestor to find.
        /// </typeparam>
        /// <param name="dependencyObject">
        /// A dependency object.
        /// </param>
        /// <param name="predicate">
        /// A predicate.
        /// </param>
        /// <returns>
        /// Returns the closest ancestor of type <typeparamref name="T"/> for which the predicate is <c>true</c>.
        /// </returns>
        /// <exception cref="Exception">
        /// Thrown when no ancestor is found of type <typeparamref name="T"/> for which the predicate is <c>true</c>.
        /// </exception>
        public static T FindVisualAncestor<T>(this DependencyObject dependencyObject, Func<T, bool> predicate)
            where T : DependencyObject
        {
            if (TryFindVisualAncestor(dependencyObject, predicate, out T ancestor))
                return ancestor;

            throw new Exception($"No ancestor found of type '{nameof(T)}' = '{typeof(T)}' for which the predicate is true.");
        }

        /// <summary>
        /// Searches the visual tree for a dependency object's closest ancestor of type <typeparamref name="T"/> for
        /// which a specified predicate is <c>true</c>.
        /// </summary>
        /// <typeparam name="T">
        /// Type of ancestor to find.
        /// </typeparam>
        /// <param name="dependencyObject">
        /// A dependency object.
        /// </param>
        /// <param name="ancestor">
        /// Returns the closest ancestor of type <typeparamref name="T"/> for which the predicate is <c>true</c>.
        /// Returns <c>null</c> if not found.
        /// </param>
        /// <returns>
        /// Returns <c>true</c> if and only if an ancestor is found of type <typeparamref name="T"/> for which the
        /// predicate is <c>true</c>.
        /// </returns>
        public static bool TryFindVisualAncestor<T>(
            this DependencyObject dependencyObject, Func<T, bool> predicate, out T ancestor) where T : DependencyObject
        {
            if (dependencyObject == null)
                throw new ArgumentNullException(nameof(dependencyObject));

            if (!TryFindVisualAncestor(dependencyObject, out ancestor))
                return false;

            if (predicate(ancestor) == true)
                return true;

            return TryFindVisualAncestor(ancestor, predicate, out ancestor);
        }

        /// <summary>
        /// Enumerates all children of an object in the visual tree.
        /// </summary>
        /// <param name="dependencyObject">
        /// A dependency object.
        /// </param>
        /// <returns>
        /// Returns all visual tree children of <paramref name="dependencyObject"/>.
        /// </returns>
        public static IEnumerable<DependencyObject> GetVisualChildren(this DependencyObject dependencyObject)
        {
            if (dependencyObject == null)
                throw new ArgumentNullException(nameof(dependencyObject));

            for (int index = 0; index < VisualTreeHelper.GetChildrenCount(dependencyObject); index++)
                yield return VisualTreeHelper.GetChild(dependencyObject, index);
        }

        /// <summary>
        /// Enumerates all descendants of an object in the visual tree, using breadth-first search.
        /// </summary>
        /// <remarks>
        /// Descendants are enumerated in breadth-first order, meaning one generation at a time.  First all
        /// children are enumerated, then all grand-children, etc.
        /// </remarks>
        /// <param name="dependencyObject">
        /// A dependency object.
        /// </param>
        /// <returns>
        /// Returns all visual tree descendants of <paramref name="dependencyObject"/> in breadth-first order.
        /// </returns>
        public static IEnumerable<DependencyObject> GetVisualDescendants(this DependencyObject dependencyObject)
        {
            if (dependencyObject == null)
                throw new ArgumentNullException(nameof(dependencyObject));

            Queue<DependencyObject> queue = new Queue<DependencyObject>();
            queue.Enqueue(dependencyObject);

            while (queue.Count > 0)
            {
                foreach (DependencyObject child in queue.Dequeue().GetVisualChildren())
                {
                    yield return child;
                    queue.Enqueue(child);
                }
            }
        }

        /// <summary>
        /// Searches the visual tree using breadth-first search for a dependency object's closest descendant
        /// of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">
        /// Type of descendant to find.
        /// </typeparam>
        /// <param name="dependencyObject">
        /// A dependency object.
        /// </param>
        /// <returns>
        /// Returns the closest descendant of type <typeparamref name="T"/>.
        /// </returns>
        /// <exception cref="Exception">
        /// Thrown when no descendant of type <typeparamref name="T"/> is found.
        /// </exception>
        public static T FindVisualDescendant<T>(this DependencyObject dependencyObject) where T : DependencyObject
        {
            if (TryFindVisualDescendant(dependencyObject, out T descendant))
                return descendant;

            throw new Exception($"No descendant found of type '{nameof(T)}' = '{typeof(T)}'.");
        }

        /// <summary>
        /// Searches the visual tree using breadth-first search for a dependency object's closest descendant
        /// of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">
        /// Type of descendant to find.
        /// </typeparam>
        /// <param name="dependencyObject">
        /// A dependency object.
        /// </param>
        /// <param name="descendant">
        /// Returns the closest descendant of type <typeparamref name="T"/>.  Returns <c>null</c> if not found.
        /// </param>
        /// <returns>
        /// Returns <c>true</c> if and only if a descendant of type <typeparamref name="T"/> is found.
        /// </returns>
        public static bool TryFindVisualDescendant<T>(this DependencyObject dependencyObject, out T descendant)
            where T : DependencyObject
        {
            return TryFindVisualDescendant(dependencyObject, (t) => true, out descendant);
        }

        /// <summary>
        /// Searches the visual tree using breadth-first search for a dependency object's closest descendant
        /// of type <typeparamref name="T"/> for which a specified predicate is <c>true</c>.
        /// </summary>
        /// <typeparam name="T">
        /// Type of descendant to find.
        /// </typeparam>
        /// <param name="dependencyObject">
        /// A dependency object.
        /// </param>
        /// <param name="predicate">
        /// A predicate.
        /// </param>
        /// <returns>
        /// Returns the closest descendant of type <typeparamref name="T"/> for which the predicate is <c>true</c>.
        /// </returns>
        /// <exception cref="Exception">
        /// Thrown when no descendant is found of type <typeparamref name="T"/> for which the predicate is <c>true</c>.
        /// </exception>
        public static T FindVisualDescendant<T>(this DependencyObject dependencyObject, Func<T, bool> predicate)
            where T : DependencyObject
        {
            if (TryFindVisualDescendant(dependencyObject, predicate, out T Descendant))
                return Descendant;

            throw new Exception($"No descendant found of type '{nameof(T)}' = '{typeof(T)}' for which the predicate is true.");
        }

        /// <summary>
        /// Searches the visual tree using breadth-first search for a dependency object's closest descendant
        /// of type <typeparamref name="T"/> for which a specified predicate is <c>true</c>.
        /// </summary>
        /// <typeparam name="T">
        /// Type of descendant to find.
        /// </typeparam>
        /// <param name="dependencyObject">
        /// A dependency object.
        /// </param>
        /// <param name="predicate">
        /// A predicate.
        /// </param>
        /// <param name="descendant">
        /// Returns the closest descendant of type <typeparamref name="T"/> for which the predicate is <c>true</c>.
        /// Returns <c>null</c> if not found.
        /// </param>
        /// <returns>
        /// Returns <c>true</c> if and only if a descendant is found of type <typeparamref name="T"/> for which
        /// the predicate is <c>true</c>.
        /// </returns>
        public static bool TryFindVisualDescendant<T>(
            this DependencyObject dependencyObject, Func<T, bool> predicate, out T descendant)
            where T : DependencyObject
        {
            if (dependencyObject == null)
                throw new ArgumentNullException(nameof(dependencyObject));

            descendant = dependencyObject
                .GetVisualDescendants()
                .OfType<T>()
                .Where(predicate)
                .FirstOrDefault();

            return descendant != null;
        }

        /// <summary>
        /// Creates a binding that uses a <see cref="DependencyObject"/> instance as the source.
        /// </summary>
        /// <returns>
        /// Returns a new binding.
        /// </returns>
        public static Binding CreateBinding(this DependencyObject source, string propertyPath, BindingMode mode)
        {
            return new Binding { Source = source, Path = new PropertyPath(propertyPath), Mode = mode };
        }

        /// <summary>
        /// Discovers dependency property definitions for a <see cref="DependencyObject"/> instance.
        /// </summary>
        /// <remarks>
        /// Results include only those dependency properties that are defined in the standard manner (i.e.: via a
        /// public static field on the class to which the property applies).  Results do not include attached
        /// properties.
        /// </remarks>
        /// <param name="dependencyObject">
        /// A dependency object.
        /// </param>
        /// <param name="includeInherited">
        /// When <c>false</c>, only dependency properties defined on the dependency object's concrete type are
        /// included.  When <c>true</c>, dependency properties defined on ancestor types are included as well.
        /// </param>
        /// <returns>
        /// Returns <see cref="FieldInfo"/> objects corresponding to public static fields of type
        /// <see cref="DependencyProperty"/> for the dependency object.
        /// </returns>
        public static IEnumerable<FieldInfo> GetDependencyProperties(
            this DependencyObject dependencyObject, bool includeInherited = true)
        {
            return GetDependencyProperties(dependencyObject.GetType(), includeInherited);
        }

        public static bool HasFocus(this DependencyObject dependencyObject)
        {
            return ReferenceEquals(FocusManager.GetFocusedElement(), dependencyObject);
        }

        public static bool VisualAncestorHasFocus(this DependencyObject dependencyObject)
        {
            object focusedElement = FocusManager.GetFocusedElement();

            return dependencyObject.TryFindVisualAncestor<DependencyObject>(
                ancestor => ReferenceEquals(ancestor, focusedElement), out _);
        }

        public static bool VisualDescendantHasFocus(this DependencyObject dependencyObject)
        {
            object focusedElement = FocusManager.GetFocusedElement();

            return dependencyObject.TryFindVisualDescendant<DependencyObject>(
                descendant => ReferenceEquals(descendant, focusedElement), out _);
        }

        public static bool TryGetDependencyPropertyValue<T>(
            this DependencyObject dependencyObject, string propertyName, out T value)
        {
            if (dependencyObject.TryGetDependencyProperty(propertyName, out DependencyProperty dependencyProperty))
            {
                value = (T)dependencyObject.GetValue(dependencyProperty);
                return true;
            }
            else
            {
                value = default;
                return false;
            }
        }

        public static bool TryGetDependencyProperty(
            this DependencyObject dependencyObject, string propertyName, out DependencyProperty property)
        {
            IEnumerable<FieldInfo> dependencyProperties = dependencyObject.GetDependencyProperties();

            property = dependencyProperties
                .Where(fieldInfo => fieldInfo.Name.Equals(propertyName))
                .Select(fieldInfo => (DependencyProperty)fieldInfo.GetValue(null))
                .FirstOrDefault();

            return property != null;
        }

        private static IEnumerable<FieldInfo> GetDependencyProperties(Type type, bool includeInherited)
        {
            IEnumerable<FieldInfo> properties = type.GetFields(BindingFlags.Static | BindingFlags.Public)
                .Where(fieldInfo => fieldInfo.FieldType == typeof(DependencyProperty));

            if (includeInherited && type.BaseType != null)
                properties = properties.Union(GetDependencyProperties(type.BaseType, true));

            return properties;
        }

        /// <summary>
        /// A replacement for <DependencyObject>.SetValue(...);
        ///
        /// This will only set the value if it's different to help prevent cycles.
        /// </summary>
        /// <returns>True if the values were different, false otherwise.</returns>
        public static bool SetIfDifferent<T>(this DependencyObject @this, DependencyProperty dp, T value, Func<T, T, bool> equalityChecker = null)
        {
            @this.ThrowIfNull(nameof(@this));
            dp.ThrowIfNull(nameof(dp));

            if (equalityChecker == null)
            {
                equalityChecker = (x, y) => Equals(x, y);
            }

            bool success;

            var originalValue = (T)@this.GetValue(dp);
            if (success = !equalityChecker.Invoke(value, originalValue))
            {
                @this.SetValue(dp, value);
            }

            return success;
        }
    }
}