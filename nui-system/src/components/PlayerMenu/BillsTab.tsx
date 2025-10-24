// BillsTab.tsx
import { FileText, CreditCard } from 'lucide-react'
import { fetchNui } from '../../utils/fetchNui'

interface Bill {
  company: string
  author: string
  date: string
  amount: number
}

interface BillsTabProps {
  bills: Bill[]
  playerMoney: number
}

export default function BillsTab({ bills, playerMoney }: BillsTabProps) {
  const handlePayBill = (billIndex: number) => {
    fetchNui('nuiCallback', {
      callback: 'player:payBill',
      billIndex
    })
  }

  if (bills.length === 0) {
    return (
      <div className="empty-state">
        <FileText size={48} />
        <h3>Aucune facture</h3>
        <p>Vous n'avez aucune facture en attente</p>
      </div>
    )
  }

  return (
    <div className="bills-grid">
      {bills.map((bill, index) => (
        <div key={index} className="bill-card">
          <div className="card-header">
            <span className="card-icon">🧾</span>
            <div style={{ flex: 1 }}>
              <h3 className="card-title">{bill.company}</h3>
              <p className="card-subtitle">Par {bill.author} · {bill.date}</p>
            </div>
            <div style={{
              background: 'rgba(220, 38, 38, 0.2)',
              color: '#fca5a5',
              padding: '6px 12px',
              borderRadius: '6px',
              fontSize: '14px',
              fontWeight: '700'
            }}>
              ${bill.amount}
            </div>
          </div>
          <button 
            className="card-btn primary"
            onClick={() => handlePayBill(index)}
            disabled={playerMoney < bill.amount}
            style={{ marginTop: '16px', width: '100%' }}
          >
            <CreditCard size={16} />
            {playerMoney >= bill.amount ? 'Payer' : 'Fonds insuffisants'}
          </button>
        </div>
      ))}
    </div>
  )
}