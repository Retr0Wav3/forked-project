﻿using System.Collections.Generic;
using System.Linq;
using Model;
using Model.Runtime.Projectiles;
using UnityEngine;

namespace UnitBrains.Player
{
    public class SecondUnitBrain : DefaultPlayerUnitBrain
    {
        private static int unitCounter = 0;
        public override string TargetUnitName => "Cobra Commando";
        private const float OverheatTemperature = 3f;
        private const float OverheatCooldown = 2f;
        private const int MaxTargetsForSelection = 4;
        private float _temperature = 0f;
        private float _cooldownTime = 0f;
        private bool _overheated;
        private int _unitID;
        private List<Vector2Int> _unreachableTargets = new ();
        
        public SecondUnitBrain()
        {
            _unitID = unitCounter++;
        }
        
        protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
        {
            float overheatTemperature = OverheatTemperature;
            float currentTemperature = GetTemperature();

            if (currentTemperature >= overheatTemperature)
                return;

            for (int i = 0; i <= currentTemperature; i++)
            {
                var projectile = CreateProjectile(forTarget);
                AddProjectileToList(projectile, intoList);
            }
            
            IncreaseTemperature();
        }

        public override Vector2Int GetNextStep()
        {
            if (SelectTargets().Any())
                return unit.Pos;
                
            return CalcNextStepTowards(_unreachableTargets.First());
        }

        protected override List<Vector2Int> SelectTargets()
        {
            List<Vector2Int> result = new();
            Vector2Int closestEnemyPosition;
            _unreachableTargets.Clear();
            
            foreach (Vector2Int target in GetAllTargetsWithoutBase())
            {
                _unreachableTargets.Add(target);
            }
            
            if (_unreachableTargets.Count == 0)
                _unreachableTargets.Add(runtimeModel.RoMap.Bases[RuntimeModel.BotPlayerId]);
            
            SortByDistanceToOwnBase(_unreachableTargets);
            
            int enemyIndex = (_unitID - 1) % Mathf.Min(MaxTargetsForSelection, _unreachableTargets.Count);
            closestEnemyPosition = _unreachableTargets[enemyIndex];

            if(IsTargetInRange(closestEnemyPosition))
                result.Add(closestEnemyPosition);

            return result;
        }

        public override void Update(float deltaTime, float time)
        {
            if (_overheated)
            {              
                _cooldownTime += Time.deltaTime;
                float t = _cooldownTime / (OverheatCooldown/10);
                _temperature = Mathf.Lerp(OverheatTemperature, 0, t);
                if (t >= 1)
                {
                    _cooldownTime = 0;
                    _overheated = false;
                }
            }
        }

        private int GetTemperature()
        {
            if(_overheated) return (int) OverheatTemperature;
            else return (int)_temperature;
        }

        private void IncreaseTemperature()
        {
            _temperature += 1f;
            if (_temperature >= OverheatTemperature) _overheated = true;
        }
    }
}