﻿#nullable enable
using System;
using System.Text.RegularExpressions;

namespace CFisobs.Core
{
    /// <summary>
    /// Represents saved information about <see cref="AbstractPhysicalObject"/> instances.
    /// </summary>
    public readonly struct EntitySaveData
    {
        private readonly Either<AbstractPhysicalObject.AbstractObjectType, CreatureTemplate.Type> type;

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
        internal EntitySaveData(Either<AbstractPhysicalObject.AbstractObjectType, CreatureTemplate.Type> type, EntityID id, WorldCoordinate pos, string customData)
        {
            this.type = type;
            ID = id;
            Pos = pos;
            CustomData = customData;
        }

        // Catches stuff like `<`, `<cA`, `<cD`, `<abc` etc
        readonly static Regex invalidCreatureData = new("<[^c]?[^B-C]?");

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

            if (apo is AbstractCreature) {
                if (invalidCreatureData.Match(customData) is Match m) {
                    throw new ArgumentException($"Creature data cannot contain certain patterns. The pattern \"{m.Value}\" is disallowed.");
                }
            }
            else if (customData.IndexOf('<') != -1) {
                throw new ArgumentException("Item data cannot contain the < character.");
            }

            if (apo is AbstractCreature crit) {
                return new EntitySaveData(crit.creatureTemplate.type, apo.ID, apo.pos, customData);
            }

            return new EntitySaveData(apo.type, apo.ID, apo.pos, customData);
        }

        /// <summary>
        /// Gets this entity's save data as a string.
        /// </summary>
        /// <returns>A string representation of this data.</returns>
        public override string ToString()
        {
            if (type.MatchR(out var critType)) {
                return $"{critType}<cA>{ID}<cA>{Pos.room}.{Pos.abstractNode}<cA>{CustomData}";
            }
            if (type.MatchL(out var objectType)) {
                string customDataStr = string.IsNullOrEmpty(CustomData) ? "" : $"<oA>{CustomData}";

                return $"{ID}<oA>{objectType}<oA>{Pos.room}.{Pos.x}.{Pos.y}.{Pos.abstractNode}{customDataStr}";
            }
            return "";
        }
    }
}