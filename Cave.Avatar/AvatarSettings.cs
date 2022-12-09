using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Cave.Media
{
    /// <summary>
    /// Provides all settings an avatar needs.
    /// </summary>
    public class AvatarSettings : IEnumerable<ValueType>
    {
        readonly Dictionary<Type, ValueType> dict = new();

        /// <summary>
        /// Event to be called whenever a setting changes.
        /// </summary>
        public event Action<ValueType> Changed;

        /// <summary>
        /// Creates a new instance of the <see cref="AvatarSettings"/> class.
        /// </summary>
        /// <param name="name">Name or email address of the user.</param>
        /// <param name="size">Size of the avatar in pixels.</param>
        public AvatarSettings(string name, int size)
        {
            Name = name;
            Size = size;
        }

        /// <summary>
        /// Gets or sets the user name or email address.
        /// This is the text that will be hashed and sent to the avatar server at most implementations.
        /// </summary>
        public string Name { get => Get<AvatarName>().Name; set => Set(new AvatarName() { Name = value }); }
        
        /// <summary>
        /// Gets or sets the size of the genrated avatar image in pixels.
        /// </summary>
        public int Size { get => Get<AvatarSize>().Size; set => Set(new AvatarSize() { Size = value }); }

        /// <summary>
        /// Adds a custom setting to the avatar.
        /// </summary>
        /// <typeparam name="T">Type of the setting.</typeparam>
        /// <param name="setting">Setting to be added.</param>
        public void Add<T>(T setting) where T : struct
        {
            dict.Add(typeof(T), setting);
            OnChanged(setting);
        }

        /// <summary>
        /// Sets a custom setting at the avatar.
        /// </summary>
        /// <typeparam name="T">Type of the setting.</typeparam>
        /// <param name="setting">Setting to be set.</param>
        public void Set<T>(T setting) where T : struct
        {
            dict[typeof(T)] = setting;
            OnChanged(setting);
        }

        /// <summary>
        /// Gets a setting. An exception is thrown if the setting is not present.
        /// </summary>
        /// <typeparam name="T">Type of the setting</typeparam>
        /// <returns>Returns the setting.</returns>
        public T Get<T>() where T : struct => (T)dict[typeof(T)];

        /// <summary>
        /// Gets a setting. An exception is thrown if the setting is not present.
        /// </summary>
        /// <typeparam name="T">Type of the setting</typeparam>
        /// <returns>Returns the setting.</returns>
        public bool TryGet<T>(out T setting) where T : struct
        {
            if (dict.TryGetValue(typeof(T), out var value) && value is T result)
            {
                setting = result;
                return true;
            }
            setting = default;
            return false;
        }

        /// <summary>
        /// Calles the <see cref="Changed"/> event.
        /// </summary>
        /// <param name="setting">New setting value.</param>
        protected virtual void OnChanged(ValueType setting) => Changed?.Invoke(setting);
        
        /// <inheritdoc/>
        public IEnumerator<ValueType> GetEnumerator() => dict.Values.GetEnumerator();

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator() => dict.Values.GetEnumerator();
    }
}
