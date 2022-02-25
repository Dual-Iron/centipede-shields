﻿using System;
using System.IO;
using System.Linq;
using UnityEngine;

namespace CFisobs
{
    /// <summary>
    /// Provides methods to simplify the creation of custom <see cref="PhysicalObject"/> and <see cref="AbstractPhysicalObject"/> types.
    /// </summary>
    public abstract class Fisob
    {
        static bool IsValidID(char c)
        {
            return c >= 'a' && c <= 'z' || c == '_';
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Fisob"/> class.
        /// </summary>
        /// <param name="id">This fisob's unique identifier. Only lowercase `a-z` and `_` are allowed.</param>
        protected Fisob(string id)
        {
            if (string.IsNullOrEmpty(id)) {
                throw new ArgumentException("ID cannot be null or empty.");
            }

            if (!id.All(IsValidID)) {
                throw new ArgumentException("ID must only consist of a-z and _.");
            }

            ID = id;

            IconName = $"icon_{ID}";
            IconColor = Menu.Menu.MenuRGB(Menu.Menu.MenuColors.MediumGrey);
        }

        private AbstractPhysicalObject.AbstractObjectType? type;

        /// <summary>
        /// The <see cref="FAtlasElement"/> name of this fisob's icon sprite.
        /// </summary>
        public string IconName { get; protected set; }

        /// <summary>
        /// The color of this fisob's icon sprite.
        /// </summary>
        public Color IconColor { get; protected set; }

        /// <summary>
        /// This fisob's unique identifier.
        /// </summary>
        public readonly string ID;

        /// <summary>
        /// This fisob's enum value.
        /// </summary> 
        /// <exception cref="ArgumentException">Thrown when the fisob has not been added to a registry yet.</exception>
        public AbstractPhysicalObject.AbstractObjectType Type {
            get {
                if (type == null) {
                    type = RWCustom.Custom.ParseEnum<AbstractPhysicalObject.AbstractObjectType>(ID);
                }
                return type.Value;
            }
        }

        /// <summary>
        /// Gets an APO from save data.
        /// </summary>
        /// <param name="world">The entity's world.</param>
        /// <param name="saveData">The entity's save data. If spawned via sandbox mode, <see cref="EntitySaveData.CustomData"/> will be empty.</param>
        /// <returns>A newly created abstract physical object, or <see langword="null"/>.</returns>
        public abstract AbstractPhysicalObject Parse(World world, EntitySaveData saveData);

        /// <summary>
        /// Gets an object representing the properties of a PO.
        /// </summary>
        /// <param name="forObject">The physical object whose properties to get.</param>
        /// <returns>An instance of <see cref="FisobProperties"/>, or <see langword="null"/>.</returns>
        public virtual FisobProperties GetProperties(PhysicalObject forObject) => null;

        /// <summary>
        /// Gets how this object should be displayed in sandbox mode.
        /// </summary>
        /// <param name="unlocks">The current save's multiplayer unlocks.</param>
        /// <returns>An instance of <see cref="SandboxState"/>.</returns>
        public virtual SandboxState GetSandboxState(MultiplayerUnlocks unlocks) => 0;

        /// <summary>
        /// Used to load atlases into <see cref="Futile.atlasManager"/> for later use.
        /// </summary>
        /// <param name="rainWorld">The application instance.</param>
        public virtual void LoadResources(RainWorld rainWorld)
        {
            if (!LoadAtlasFromEmbeddedResource(IconName)) {
                IconName = "Futile_White";
            }
        }

        /// <summary>
        /// Loads an embedded resource named <paramref name="resource"/> into <see cref="Futile.atlasManager"/> if the resource exists.
        /// </summary>
        /// <param name="resource">The name of the embedded resource.</param>
        /// <returns>If the resource was successfully loaded, <see langword="true"/>; otherwise, <see langword="false"/>.</returns>
        protected bool LoadAtlasFromEmbeddedResource(string resource)
        {
            using (Stream stream = typeof(Fisob).Assembly.GetManifestResourceStream(resource)) {
                if (stream == null) {
                    return false;
                }

                byte[] image = new byte[stream.Length];

                stream.Read(image, 0, image.Length);

                Texture2D tex = new Texture2D(1, 1);

                tex.LoadImage(image);

                Futile.atlasManager.LoadAtlasFromTexture(resource, tex);

                return true;
            }
        }
    }
}
