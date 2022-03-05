using System;
using System.Collections.Generic;

namespace CFisobs
{
    /// <summary>
    /// Provides methods to simplify the creation of custom <see cref="PhysicalObject"/> and <see cref="AbstractPhysicalObject"/> types.
    /// </summary>
    public abstract class Fisob
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Fisob"/> class.
        /// </summary>
        /// <param name="id">This fisob's unique identifier. Only lowercase `a-z` and `_` are allowed.</param>
        protected Fisob(string id)
        {
            ID = id;
        }

        /// <summary>
        /// This fisob's unique identifier.
        /// </summary>
        public readonly string ID;

        public readonly List<SandboxUnlock> SandboxUnlocks = new List<SandboxUnlock>();

        public IFisobIcon Icon;

        private AbstractPhysicalObject.AbstractObjectType? type;

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
        /// Used to load atlases into <see cref="Futile.atlasManager"/> for later use.
        /// </summary>
        /// <param name="rainWorld">The application instance.</param>
        public virtual void LoadResources(RainWorld rainWorld)
        {
            string iconName = FisobExtensions.LoadAtlasFromEmbeddedResource($"icon_{ID}") ? $"icon_{ID}" : "Futile_White";

            if (Icon == null) {
                Icon = new SimpleFisobIcon(iconName, FisobExtensions.DefaultIconColor);
            }
        }
    }
}
