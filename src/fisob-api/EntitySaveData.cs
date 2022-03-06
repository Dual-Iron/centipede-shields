using System;

namespace CFisobs
{
    /// <summary>
    /// Represents saved information about <see cref="AbstractPhysicalObject"/> instances.
    /// </summary>
    public readonly struct EntitySaveData
    {
        private readonly AbstractPhysicalObject.AbstractObjectType objType;
        private readonly CreatureTemplate.Type critType;

        /// <summary>
        /// The APO's ID.
        /// </summary>
        public readonly EntityID ID;

        /// <summary>
        /// The APO's position.
        /// </summary>
        public readonly WorldCoordinate Pos;

        /// <summary>
        /// Any extra data associated with the APO. This can be <see cref="string.Empty"/>, but not <see langword="null"/>.
        /// </summary>
        /// <remarks>For creatures, this will be a stringified <see cref="CreatureState"/>.</remarks>
        public readonly string CustomData;

        /// <summary>
        /// Initializes a new instance of the <see cref="EntitySaveData"/> struct.
        /// </summary>
        /// <remarks>Do not use this constructor. Call <see cref="CreateFrom(AbstractPhysicalObject, string)"/> instead.</remarks>
        internal EntitySaveData(AbstractPhysicalObject.AbstractObjectType objectType, CreatureTemplate.Type creatureType, EntityID id, WorldCoordinate pos, string customData)
        {
            objType = objectType;
            critType = creatureType;
            ID = id;
            Pos = pos;
            CustomData = customData;
        }

        /// <summary>
        /// Creates an instance of the <see cref="EntitySaveData"/> struct.
        /// </summary>
        /// <param name="apo">The abstract physical object to get basic data from.</param>
        /// <param name="customData">Extra data associated with the abstract physical object. This data should never contain &lt; characters.</param>
        /// <returns>A new instance of <see cref="EntitySaveData"/>.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="customData"/> contains &lt; characters.</exception>
        public static EntitySaveData CreateFrom(AbstractPhysicalObject apo, string customData = "")
        {
            if (customData is null) {
                throw new ArgumentNullException(nameof(customData));
            }

            if (customData.IndexOf('<') != -1) {
                throw new ArgumentException("Custom data cannot contain < characters.");
            }

            if (apo is AbstractCreature crit) {
                return new EntitySaveData(AbstractPhysicalObject.AbstractObjectType.Creature, crit.creatureTemplate.type, apo.ID, apo.pos, customData);
            }

            return new EntitySaveData(apo.type, 0, apo.ID, apo.pos, customData);
        }

        /// <summary>
        /// Gets this entity's save data as a string.
        /// </summary>
        /// <returns>A string representation of this data.</returns>
        public override string ToString()
        {
            if (objType == AbstractPhysicalObject.AbstractObjectType.Creature) {
                return $"{critType}<cA>{ID}<cA>{Pos.room}.{Pos.abstractNode}<cA>{CustomData}";
            }
            string customDataStr = string.IsNullOrEmpty(CustomData) ? "" : $"<oA>{CustomData}";
            return $"{ID}<oA>{objType}<oA>{Pos.room}.{Pos.x}.{Pos.y}.{Pos.abstractNode}{customDataStr}";
        }
    }
}