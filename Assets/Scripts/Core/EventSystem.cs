using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SpaceBoat.Ship.Activatables;
using SpaceBoat.PlayerSubclasses.Equipment;

namespace SpaceBoat {
    public class EventContext {
        private GameModel model;
        public GameModel Model {get {return model;}}
        private Player player;
        public Player Player {get {return player;}}
        private Player.PlayerMovementInfo playerMovementInfo;
        public Player.PlayerMovementInfo PlayerMovementInfo {get {return playerMovementInfo;}}
        private PlayerStateName playerState;
        public PlayerStateName PlayerState {get {return playerState;}}
        private IActivatables activatable;
        public IActivatables Activatable {get {return activatable;}}
        private Rewards.ICraftBlueprint craftBlueprint;
        public Rewards.ICraftBlueprint CraftBlueprint {get {return craftBlueprint;}}
        private IPlayerEquipment equipment;
        public IPlayerEquipment Equipment {get {return equipment;}}
        private Ship.Buildables.IBuildable buildable;
        public Ship.Buildables.IBuildable Buildable {get {return buildable;}}

        


        public EventContext(GameModel model, params object[] args) {
            this.model = model;
            //this is kinda ugly but I'm not sure if there is a cleaner way to do this.
            foreach (object arg in args) {
                switch (arg) {
                    case Player player:
                        this.player = player;
                        break;
                    case Player.PlayerMovementInfo playerMovementInfo:
                        this.playerMovementInfo = playerMovementInfo;
                        break;
                    case PlayerStateName playerState:
                        this.playerState = playerState;
                        break;
                    case IActivatables activatable:
                        this.activatable = activatable;
                        break;
                    case Rewards.ICraftBlueprint craftBlueprint:
                        this.craftBlueprint = craftBlueprint;
                        break;
                    case IPlayerEquipment equipment:
                        this.equipment = equipment;
                        break;
                    case Ship.Buildables.IBuildable buildable:
                        this.buildable = buildable;
                        break;
                    default:
                        Debug.LogWarning("EventContext: Unhandled argument type: " + arg.GetType());
                        break;
                }
            }
        }
    }
}