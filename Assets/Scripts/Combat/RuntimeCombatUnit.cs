using UnityEngine;

namespace PickMeUp.Combat
{
    public enum UnitTeam { Player, Enemy }
    public enum UnitState { Idle, Attacking, Defending, Dead }

    public class RuntimeCombatUnit : MonoBehaviour
    {
        [Header("Identity")]
        public string unitId;
        public string displayName;
        public UnitTeam team;
        public int level = 1;

        [Header("Stats")]
        public int maxHP = 100;
        public int currentHP;
        public int attack = 10;
        public int defense = 5;
        public int speed = 10;
        public int critRate = 5; // percent

        [Header("Skill")]
        public string skillName = "Power Strike";
        public int skillCooldown = 3;
        public int skillCurrentCooldown = 0;
        public float skillMultiplier = 1.5f;

        [Header("State")]
        public UnitState state = UnitState.Idle;
        public bool isDefending = false;
        public bool isDead => currentHP <= 0;

        [Header("Visual")]
        public Sprite portrait;
        public Color unitColor = Color.white;

        public System.Action<int> OnHPChanged;
        public System.Action OnDeath;
        public System.Action OnTurnStart;
        public System.Action OnTurnEnd;

        public void Initialize(string id, string name, UnitTeam team, int lvl, int hp, int atk, int def, int spd)
        {
            unitId = id;
            displayName = name;
            this.team = team;
            level = lvl;
            maxHP = hp;
            currentHP = hp;
            attack = atk;
            defense = def;
            speed = spd;
            state = UnitState.Idle;
            isDefending = false;
        }

        public void TakeDamage(int damage)
        {
            if (isDead) return;

            int actualDamage = Mathf.Max(1, damage - (isDefending ? defense * 2 : defense));
            currentHP = Mathf.Max(0, currentHP - actualDamage);

            OnHPChanged?.Invoke(currentHP);

            if (currentHP <= 0)
            {
                state = UnitState.Dead;
                OnDeath?.Invoke();
            }
        }

        public void Heal(int amount)
        {
            if (isDead) return;
            currentHP = Mathf.Min(maxHP, currentHP + amount);
            OnHPChanged?.Invoke(currentHP);
        }

        public int CalculateDamage(RuntimeCombatUnit target, bool isSkill = false)
        {
            float multiplier = isSkill ? skillMultiplier : 1.0f;
            bool isCrit = Random.Range(0, 100) < critRate;
            if (isCrit) multiplier *= 1.5f;

            int baseDamage = Mathf.FloorToInt(attack * multiplier);
            return baseDamage;
        }

        public void StartTurn()
        {
            if (isDead) return;
            isDefending = false;
            state = UnitState.Idle;
            if (skillCurrentCooldown > 0) skillCurrentCooldown--;
            OnTurnStart?.Invoke();
        }

        public void EndTurn()
        {
            OnTurnEnd?.Invoke();
        }

        public void SetDefending()
        {
            state = UnitState.Defending;
            isDefending = true;
        }

        public void UseSkill()
        {
            skillCurrentCooldown = skillCooldown;
            state = UnitState.Attacking;
        }

        public bool CanUseSkill()
        {
            return skillCurrentCooldown <= 0 && !isDead;
        }

        public float GetHPRatio()
        {
            return maxHP > 0 ? (float)currentHP / maxHP : 0f;
        }
    }
}
