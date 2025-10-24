// WeaponsTab.tsx
import { Crosshair } from 'lucide-react'
import { fetchNui } from '../../utils/fetchNui'

interface Weapon {
  name: string
  ammo: number
  icon: string
}

interface WeaponsTabProps {
  weapons: Weapon[]
}

export default function WeaponsTab({ weapons }: WeaponsTabProps) {
  const handleToggleWeapon = (weaponName: string, action: 'equip' | 'unequip') => {
    fetchNui('nuiCallback', {
      callback: 'player:toggleWeapon',
      weaponName,
      action
    })
  }

  if (weapons.length === 0) {
    return (
      <div className="empty-state">
        <Crosshair size={48} />
        <h3>Aucune arme</h3>
        <p>Vous n'avez aucune arme dans votre inventaire</p>
      </div>
    )
  }

  return (
    <div className="weapons-grid">
      {weapons.map((weapon, index) => (
        <div key={index} className="weapon-card">
          <div className="card-header">
            <span className="card-icon">{weapon.icon}</span>
            <div>
              <h3 className="card-title">{weapon.name}</h3>
              <p className="card-subtitle">Munitions: {weapon.ammo}</p>
            </div>
          </div>
          <div className="card-actions">
            <button 
              className="card-btn primary"
              onClick={() => handleToggleWeapon(weapon.name, 'equip')}
            >
              Équiper
            </button>
            <button 
              className="card-btn secondary"
              onClick={() => handleToggleWeapon(weapon.name, 'unequip')}
            >
              Ranger
            </button>
          </div>
        </div>
      ))}
    </div>
  )
}
