import { useState } from 'react'
import { X, Wallet, Building2, ArrowDownToLine, ArrowUpFromLine } from 'lucide-react'
import { fetchNui } from '../utils/fetchNui'
import { useNuiEvent } from '../types/useNuiEvent'

interface BankProps {
  data: {
    bankName: string
    moneyInBank: number
    moneyInPocket: number
    playerName: string
  }
  onClose: () => void
}

type TransactionType = 'deposit' | 'withdraw'

export default function Bank({ data, onClose }: BankProps) {
  const [amount, setAmount] = useState('')
  const [activeTab, setActiveTab] = useState<TransactionType>('deposit')
  const [currentMoneyInBank, setCurrentMoneyInBank] = useState(data.moneyInBank)
  const [currentMoneyInPocket, setCurrentMoneyInPocket] = useState(data.moneyInPocket)

  const formatMoney = (value: number) => {
    return `$${value.toLocaleString('en-US')}`
  }

  const handleTransaction = () => {
    const numAmount = parseInt(amount)
    if (isNaN(numAmount) || numAmount <= 0) {
      console.error('[BANK UI] Montant invalide')
      return
    }

    if (activeTab === 'withdraw' && numAmount > currentMoneyInBank) {
      console.error('[BANK UI] Solde bancaire insuffisant')
      return
    }

    if (activeTab === 'deposit' && numAmount > currentMoneyInPocket) {
      console.error('[BANK UI] Argent liquide insuffisant')
      return
    }

    console.log('[BANK UI] Envoi du callback avec:', {
      callback: 'bank:transaction',
      action: activeTab,
      amount: numAmount
    })

    fetchNui('nuiCallback', {
      callback: 'bank:transaction',
      action: activeTab,
      amount: numAmount
    }).then(() => {
      console.log('[BANK UI] Callback envoyé avec succès')
    }).catch(err => {
      console.error('[BANK UI] Erreur lors de l\'envoi:', err)
    })

    setAmount('')
  }

  const handleQuickAmount = (value: number) => {
    setAmount(value.toString())
  }

  const quickAmounts = [100, 500, 1000, 5000, 10000]

  useNuiEvent('updateData', (data: any) => {
    if (data.data) {
      updateData(data.data.moneyInBank, data.data.moneyInPocket)
    }
  })

  const updateData = (newMoneyInBank: number, newMoneyInPocket: number) => {
    setCurrentMoneyInBank(newMoneyInBank)
    setCurrentMoneyInPocket(newMoneyInPocket)
  }

  return (
    <div className="bank-container">
      <div className="bank-card">
        <div className="bank-header">
          <div className="bank-header-content">
            <Building2 className="bank-icon" size={24} />
            <div>
              <h1 className="bank-title">{data.bankName}</h1>
              <p className="bank-subtitle">Bienvenue, {data.playerName}</p>
            </div>
          </div>
          <button onClick={onClose} className="close-button">
            <X size={20} />
          </button>
        </div>

        <div className="balance-grid">
          <div className="balance-card bank">
            <div className="balance-icon-wrapper">
              <Building2 size={20} />
            </div>
            <div>
              <p className="balance-label">Compte bancaire</p>
              <p className="balance-amount">{formatMoney(currentMoneyInBank)}</p>
            </div>
          </div>

          <div className="balance-card cash">
            <div className="balance-icon-wrapper">
              <Wallet size={20} />
            </div>
            <div>
              <p className="balance-label">Argent liquide</p>
              <p className="balance-amount">{formatMoney(currentMoneyInPocket)}</p>
            </div>
          </div>
        </div>

        <div className="tabs">
          <button
            className={`tab ${activeTab === 'deposit' ? 'active' : ''}`}
            onClick={() => setActiveTab('deposit')}
          >
            <ArrowDownToLine size={18} />
            Déposer
          </button>
          <button
            className={`tab ${activeTab === 'withdraw' ? 'active' : ''}`}
            onClick={() => setActiveTab('withdraw')}
          >
            <ArrowUpFromLine size={18} />
            Retirer
          </button>
        </div>

        <div className="transaction-form">
          <div className="input-group">
            <label className="input-label">Montant</label>
            <div className="input-wrapper">
              <span className="input-prefix">$</span>
              <input
                type="number"
                value={amount}
                onChange={(e) => setAmount(e.target.value)}
                placeholder="0"
                className="input-field"
                min="0"
              />
            </div>
          </div>

          <div className="quick-amounts">
            <p className="quick-amounts-label">Montants rapides</p>
            <div className="quick-amounts-grid">
              {quickAmounts.map((value) => (
                <button
                  key={value}
                  onClick={() => handleQuickAmount(value)}
                  className="quick-amount-button"
                >
                  {formatMoney(value)}
                </button>
              ))}
            </div>
          </div>

          <button
            onClick={handleTransaction}
            className={`submit-button ${activeTab === 'deposit' ? 'deposit' : 'withdraw'}`}
            disabled={!amount || parseInt(amount) <= 0}
          >
            {activeTab === 'deposit' ? 'Déposer l\'argent' : 'Retirer l\'argent'}
          </button>
        </div>

        <div className="bank-footer">
          <p className="footer-text">
            Toutes les transactions sont sécurisées et enregistrées
          </p>
        </div>
      </div>
    </div>
  )
}