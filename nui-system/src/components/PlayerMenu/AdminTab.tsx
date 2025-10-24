import { useState } from 'react'
import { Zap, Car, Navigation, Ban, AlertTriangle, DollarSign, Heart, UserPlus, Hand } from 'lucide-react'
import { fetchNui } from '../../utils/fetchNui'
import InputModal from '../InputModal'

interface AdminTabProps {
  players: Array<any>
}

interface InputModalState {
  isOpen: boolean
  title: string
  placeholder: string
  maxLength: number
  action: string
  player?: any
}

export default function AdminTab({ players }: AdminTabProps) {
  const [inputModal, setInputModal] = useState<InputModalState>({
    isOpen: false,
    title: '',
    placeholder: '',
    maxLength: 100,
    action: ''
  })

  const openInputModal = (title: string, action: string, player?: any, placeholder: string = '', maxLength: number = 100) => {
    console.log('[AdminTab] Opening modal:', { title, action, player })
    setInputModal({
      isOpen: true,
      title,
      placeholder,
      maxLength,
      action,
      player
    })
  }

  const closeInputModal = () => {
    console.log('[AdminTab] Closing modal')
    setInputModal({
      isOpen: false,
      title: '',
      placeholder: '',
      maxLength: 100,
      action: ''
    })
  }

  const handleInputConfirm = (value: string) => {
    console.log('[AdminTab] Input confirmed:', value)
    const { action, player } = inputModal
    
    switch (action) {
      case 'jail':
        const parts = value.split(' ')
        const duration = parseInt(parts[0]) || 300
        const reason = parts.slice(1).join(' ') || 'Aucune raison'
        fetchNui('nuiCallback', {
          callback: 'admin:jail',
          targetId: player.id.toString(),
          duration,
          reason
        })
        break
      
      case 'giveMoney':
        fetchNui('nuiCallback', {
          callback: 'admin:giveMoney',
          targetId: player.id.toString(),
          amount: parseInt(value) || 0
        })
        break
      
      case 'warn':
        fetchNui('nuiCallback', {
          callback: 'admin:warn',
          targetId: player.id.toString(),
          reason: value
        })
        break
      
      case 'kick':
        fetchNui('nuiCallback', {
          callback: 'admin:kick',
          targetId: player.id.toString(),
          reason: value
        })
        break
      
      case 'ban':
        fetchNui('nuiCallback', {
          callback: 'admin:ban',
          targetId: player.id.toString(),
          reason: value
        })
        break
      
      case 'spawnVehicle':
        fetchNui('nuiCallback', {
          callback: 'admin:spawnVehicle',
          vehicleName: value
        })
        break
    }
    
    closeInputModal()
  }

  const handleAdminAction = (action: string) => {
    if (action === 'spawnVehicle') {
      openInputModal('Spawn un véhicule', 'spawnVehicle', undefined, 'Nom du véhicule (ex: adder)', 20)
    } else {
      fetchNui('nuiCallback', {
        callback: `admin:${action}`
      })
    }
  }

  const handlePlayerAction = (action: string, player: any) => {
    console.log('[AdminTab] Player action:', action, player)
    
    switch (action) {
      case 'jail':
        openInputModal(
          `Jail - ${player.firstname} ${player.lastname}`, 
          'jail', 
          player, 
          'Durée(s) Raison (ex: 300 Freekill)', 
          100
        )
        break
      
      case 'giveMoney':
        openInputModal(
          `Donner de l'argent - ${player.firstname} ${player.lastname}`, 
          'giveMoney', 
          player, 
          'Montant à donner', 
          12
        )
        break
      
      case 'warn':
        openInputModal(
          `Warn - ${player.firstname} ${player.lastname}`, 
          'warn', 
          player, 
          'Raison du warn', 
          100
        )
        break
      
      case 'kick':
        openInputModal(
          `Kick - ${player.firstname} ${player.lastname}`, 
          'kick', 
          player, 
          'Raison du kick', 
          100
        )
        break
      
      case 'ban':
        openInputModal(
          `Ban - ${player.firstname} ${player.lastname}`, 
          'ban', 
          player, 
          'Raison du ban', 
          100
        )
        break
      
      default:
        fetchNui('nuiCallback', {
          callback: `admin:${action}`,
          targetId: player.id.toString()
        })
    }
  }

  return (
    <div>
      <InputModal
        isOpen={inputModal.isOpen}
        title={inputModal.title}
        placeholder={inputModal.placeholder}
        maxLength={inputModal.maxLength}
        onConfirm={handleInputConfirm}
        onCancel={closeInputModal}
      />
      
      <div className="admin-header">
        <h2 style={{ color: 'white', fontSize: '20px', fontWeight: '700', margin: '0 0 16px 0' }}>
          Panel Administration
        </h2>
        <p style={{ color: 'rgba(255, 255, 255, 0.6)', fontSize: '14px', margin: '0 0 24px 0' }}>
          Gestion du serveur et des joueurs
        </p>
      </div>

      {/* Actions générales */}
      <div style={{ marginBottom: '32px' }}>
        <h3 style={{ color: 'rgba(255, 255, 255, 0.8)', fontSize: '14px', fontWeight: '600', marginBottom: '12px', textTransform: 'uppercase', letterSpacing: '0.5px' }}>
          Actions Générales
        </h3>
        <div className="admin-controls">
          <button className="admin-btn noclip" onClick={() => handleAdminAction('toggleNoclip')}>
            <Zap size={16} />
            Toggle NoClip
          </button>
          <button className="admin-btn spawn" onClick={() => handleAdminAction('spawnVehicle')}>
            <Car size={16} />
            Spawn Véhicule
          </button>
        </div>
      </div>

      {/* Liste des joueurs */}
      <div style={{ marginBottom: '24px' }}>
        <h3 style={{ color: 'rgba(255, 255, 255, 0.8)', fontSize: '14px', fontWeight: '600', marginBottom: '12px', textTransform: 'uppercase', letterSpacing: '0.5px' }}>
          Joueurs Connectés ({players.length})
        </h3>
      </div>

      {players.length === 0 ? (
        <div style={{
          background: 'rgba(255, 255, 255, 0.05)',
          border: '1px solid rgba(255, 255, 255, 0.1)',
          borderRadius: '12px',
          padding: '40px',
          textAlign: 'center'
        }}>
          <p style={{ color: 'rgba(255, 255, 255, 0.6)', margin: 0 }}>
            Aucun joueur connecté ou erreur de chargement
          </p>
        </div>
      ) : (
        <div className="players-table">
          <div className="table-header">
            <div className="table-header-cell">ID</div>
            <div className="table-header-cell">Nom</div>
            <div className="table-header-cell">Job</div>
            <div className="table-header-cell">Argent</div>
            <div className="table-header-cell">Bitcoin</div>
            <div className="table-header-cell">Actions</div>
          </div>
          {players.map((player, index) => (
            <div key={index} className="table-row">
              <div className="table-cell">{player.id}</div>
              <div className="table-cell">{player.firstname} {player.lastname}</div>
              <div className="table-cell">{player.job}</div>
              <div className="table-cell">${player.money}</div>
              <div className="table-cell">{player.bitcoin} BTC</div>
              <div className="table-cell">
                <div className="player-actions">
                  <button 
                    className="action-icon-btn heal"
                    onClick={() => handlePlayerAction('revive', player)}
                    title="Soigner"
                  >
                    <Heart size={16} />
                  </button>
                  <button 
                    className="action-icon-btn goto"
                    onClick={() => handlePlayerAction('goto', player)}
                    title="Aller vers"
                  >
                    <Navigation size={16} />
                  </button>
                  <button 
                    className="action-icon-btn bring"
                    onClick={() => handlePlayerAction('bring', player)}
                    title="Téléporter vers moi"
                  >
                    <UserPlus size={16} />
                  </button>
                  <button 
                    className="action-icon-btn cuff"
                    onClick={() => handlePlayerAction('cuff', player)}
                    title="Menotter"
                  >
                    <Hand size={16} />
                  </button>
                  <button 
                    className="action-icon-btn jail"
                    onClick={() => handlePlayerAction('jail', player)}
                    title="Jail"
                  >
                    🔒
                  </button>
                  <button 
                    className="action-icon-btn money"
                    onClick={() => handlePlayerAction('giveMoney', player)}
                    title="Donner argent"
                  >
                    <DollarSign size={16} />
                  </button>
                  <button 
                    className="action-icon-btn warn"
                    onClick={() => handlePlayerAction('warn', player)}
                    title="Warn"
                  >
                    <AlertTriangle size={16} />
                  </button>
                  <button 
                    className="action-icon-btn kick"
                    onClick={() => handlePlayerAction('kick', player)}
                    title="Kick"
                  >
                    ❌
                  </button>
                  <button 
                    className="action-icon-btn ban"
                    onClick={() => handlePlayerAction('ban', player)}
                    title="Ban"
                  >
                    <Ban size={16} />
                  </button>
                </div>
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  )
}