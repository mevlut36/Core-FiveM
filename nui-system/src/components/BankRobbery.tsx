import { X, Skull, Laptop, Drill, AlertTriangle, CheckCircle } from 'lucide-react'
import { fetchNui } from '../utils/fetchNui'

interface BankRobberyProps {
  data: {
    bankName: string
    hasPC: boolean
    hasDrill: boolean
  }
  onClose: () => void
}

export default function BankRobbery({ data, onClose }: BankRobberyProps) {
  const canRob = data.hasPC && data.hasDrill

  const handleStartRobbery = () => {
    if (!canRob) return

    fetchNui('nuiCallback', {
      callback: 'bank:startRobbery'
    })
  }

  return (
    <div className="bank-container">
      <div className="bank-card robbery">
        <div className="bank-header robbery-header">
          <div className="bank-header-content">
            <Skull className="bank-icon" size={24} />
            <div>
              <h1 className="bank-title">Braquage - {data.bankName}</h1>
              <p className="bank-subtitle">Zone à haut risque</p>
            </div>
          </div>
          <button onClick={onClose} className="close-button">
            <X size={20} />
          </button>
        </div>

        <div className="robbery-warning">
          <AlertTriangle size={20} />
          <div>
            <p className="warning-title">Attention !</p>
            <p className="warning-text">
              Cette action est illégale et alertera la police
            </p>
          </div>
        </div>

        <div className="robbery-requirements">
          <h3 className="requirements-title">Équipement requis</h3>
          
          <div className="requirement-list">
            <div className={`requirement-item ${data.hasPC ? 'available' : 'missing'}`}>
              <Laptop size={24} />
              <div className="requirement-info">
                <p className="requirement-name">Ordinateur portable</p>
                <p className="requirement-status">
                  {data.hasPC ? 'Disponible' : 'Manquant'}
                </p>
              </div>
              {data.hasPC ? (
                <CheckCircle size={20} className="check-icon" />
              ) : (
                <X size={20} className="cross-icon" />
              )}
            </div>

            <div className={`requirement-item ${data.hasDrill ? 'available' : 'missing'}`}>
              <Drill size={24} />
              <div className="requirement-info">
                <p className="requirement-name">Perceuse industrielle</p>
                <p className="requirement-status">
                  {data.hasDrill ? 'Disponible' : 'Manquant'}
                </p>
              </div>
              {data.hasDrill ? (
                <CheckCircle size={20} className="check-icon" />
              ) : (
                <X size={20} className="cross-icon" />
              )}
            </div>
          </div>
        </div>

        {!canRob && (
          <div className="robbery-error">
            <AlertTriangle size={18} />
            <p>Vous n'avez pas tout l'équipement nécessaire</p>
          </div>
        )}

        <div className="robbery-action">
          <button
            onClick={handleStartRobbery}
            className={`robbery-button ${canRob ? '' : 'disabled'}`}
            disabled={!canRob}
          >
            <Skull size={20} />
            {canRob ? 'Commencer le braquage' : 'Équipement manquant'}
          </button>
        </div>

        <div className="bank-footer robbery-footer">
          <p className="footer-text">
            Le braquage prendra plusieurs minutes et peut être interrompu
          </p>
        </div>
      </div>
    </div>
  )
}