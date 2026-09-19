'use client';

import { FormEvent, useState, ChangeEvent } from 'react';
import styles from './page.module.css';
type ChatMessage = {
  role: string;
    content: string;
    x: number|null;
    y: number|null;
    z: number|null;
};

export default function Home() {
  const [input, setInput] = useState('');
  const [loading, setLoading] = useState(false);
  const [messages, setMessages] = useState<ChatMessage[]>([
    {
      role: 'assistant',
      content: 'Hello! I am FairAI. Ask me about fairness, trust, or AI governance.',
    },
  ]);
 const [username, setUsername] = useState<string>('');

  // 2. Type the event as a ChangeEvent for an HTMLInputElement
  const handleChange = (event: ChangeEvent<HTMLInputElement>): void => {
    setUsername(event.target.value);
  };
  async function handleSubmit(event: FormEvent) {
    event.preventDefault();
    const trimmed = input.trim();
    if (!trimmed || loading) return;

    const apiBaseUrl =
      process.env.NEXT_PUBLIC_API_URL ||
      (typeof window !== 'undefined' && window.location.hostname.endsWith('.app.github.dev'))
        ? `https://${window.location.hostname.replace('-3000.', '-5124.')}`
        : (typeof window !== 'undefined' ? window.location.origin : 'http://localhost:5124');

    setMessages((current) => [...current, { role: 'user', content: trimmed }]);
    setInput('');
    setLoading(true);

    try {
      const response = await fetch(`${apiBaseUrl}/api/chat`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ message: trimmed, username: username }), // Include the username in the request body
      });

      if (!response.ok) {
        throw new Error('Request failed');
      }

      const data = await response.json();
      setMessages((current) => [...current, { role: 'assistant', content: data.message, x: data.x, y: data.y, z: data.z }]);
    } catch (error) {
      setMessages((current) => [
        ...current,
        { role: 'assistant', content: 'FairAI is unavailable right now. Please make sure the API is running.' },
      ]);
    } finally {
      setLoading(false);
    }
  }
    const handleVote = async (x: number, y: number, z: number, voteType: 'up' | 'down') => {
        try {
            // Fetch new coordinates from your backend API based on the vote
            const response = await fetch(`/api/vote`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ x, y, z, type: voteType }),
            });

            const data = await response.json(); // Expected: { x: number, y: number, z: number }

            // Update the specific message in the list with the fetched x, y, z

        } catch (error) {
            console.error("Failed to fetch coordinate update:", error);
        }
    };
  return (
    <main className={styles.page}>
      <section className={styles.shell}>
        <header className={styles.header}>
          <div>
            <p className={styles.eyebrow}>Governance AI</p>
            <h1>  
              <img
                src="/logo.png"
                alt="FairAI Platform Logo"
                width={36}
                height={36}
                style={{ borderRadius: '8px', objectFit: 'cover' }}
              />
              FairAI Chat</h1>
          </div>
          <span className={styles.status}>Live</span>
        </header>

        <div className={styles.chatWindow}>
          {messages.map((message, index) => (
            <div key={`${message.role}-${index}`} className={`${styles.messageRow} ${styles[message.role]}`}>
              <div className={styles.avatar}>{message.role === 'assistant' ? 'FA' : 'YO'}</div>
                  <div className={styles.bubble}>{message.content}</div>
                  {message.role === 'assistant' && (
                      <>
                          <button
                              onClick={() => handleVote(message.x, message.y, message.z, 'up')}
                              className="px-3 py-1 bg-green-500 text-white rounded text-sm hover:bg-green-600"
                          >
                              Vote ++
                          </button>
                          <button
                              onClick={() => handleVote(message.x, message.y, message.z, 'down')}
                              className="px-3 py-1 bg-red-500 text-white rounded text-sm hover:bg-red-600"
                          >
                              Vote --
                          </button>
                      </>
                  )}
            </div>
          ))}
          {loading && <div className={`${styles.messageRow} ${styles.assistant}`}><div className={styles.avatar}>FA</div><div className={styles.bubble}>Thinking…</div></div>}
        </div>

        <form className={styles.form} onSubmit={handleSubmit}>
          <input
            value={input}
            onChange={(event) => setInput(event.target.value)}
            placeholder="Type your message..."
            aria-label="Chat input"
          />
          <button type="submit" disabled={loading || !input.trim()}>
            {loading ? 'Sending...' : 'Send'}
          </button>
        </form>
<div style={{ display: 'flex', flexWrap: 'wrap'}}>
  <div style={{backgroundColor: 'lightcoral' }}><a href="https://nowpayments.io/payment/?iid=5715057547&source=button" target="_blank" rel="noreferrer noopener">
  <img src="https://nowpayments.io/images/embeds/payments-button-black.svg" alt="Crypto payment button by NOWPayments" />
</a></div>
  <div style={{ backgroundColor: 'lightgreen' }}><a href="https://nowpayments.io/payment/?iid=4914149117&source=button" target="_blank" rel="noreferrer noopener">
  <img src="https://nowpayments.io/images/embeds/payments-button-white.svg" alt="Cryptocurrency & Bitcoin payment button by NOWPayments" />
</a></div>
  <div>
      <label htmlFor="username-input" style={{ fontWeight: 'bold', color: '#555'  }}>
        Username:
      </label>
      
      <input
        id="username-input"
        type="text"
        value={username}
        onChange={handleChange}
        placeholder="Enter your username"
        style={{ padding: '8px', borderRadius: '4px', border: '1px solid #ccc', backgroundColor: 'black', color: 'white'}}
      />
      
      <p style={{ fontSize: '14px', color: '#555' }}>
        Current state: <strong>{username}</strong>
      </p>
    </div>
</div>  

     </section>
    </main>
  );
}

  return (
    <main className={styles.page}>
      <section className={styles.shell}>
        <header className={styles.header}>
          <div>
            <p className={styles.eyebrow}>Governance AI</p>
            <h1>  
              <img
                src="/logo.png"
                alt="FairAI Platform Logo"
                width={36}
                height={36}
                style={{ borderRadius: '8px', objectFit: 'cover' }}
              />
              FairAI Chat</h1>
          </div>
          <span className={styles.status}>Live</span>
        </header>

        <div className={styles.chatWindow}>
          {messages.map((message, index) => (
            <div key={`${message.role}-${index}`} className={`${styles.messageRow} ${styles[message.role]}`}>
              <div className={styles.avatar}>{message.role === 'assistant' ? 'FA' : 'YO'}</div>
              <div className={styles.bubble}>{message.content}</div>
            </div>
          ))}
          {loading && <div className={`${styles.messageRow} ${styles.assistant}`}><div className={styles.avatar}>FA</div><div className={styles.bubble}>Thinking…</div></div>}
        </div>

        <form className={styles.form} onSubmit={handleSubmit}>
          <input
            value={input}
            onChange={(event) => setInput(event.target.value)}
            placeholder="Type your message..."
            aria-label="Chat input"
          />
          <button type="submit" disabled={loading || !input.trim()}>
            {loading ? 'Sending...' : 'Send'}
          </button>
        </form>
<div style={{ display: 'flex', flexWrap: 'wrap'}}>
  <div style={{backgroundColor: 'lightcoral' }}><a href="https://nowpayments.io/payment/?iid=5715057547&source=button" target="_blank" rel="noreferrer noopener">
  <img src="https://nowpayments.io/images/embeds/payments-button-black.svg" alt="Crypto payment button by NOWPayments" />
</a></div>
  <div style={{ backgroundColor: 'lightgreen' }}><a href="https://nowpayments.io/payment/?iid=4914149117&source=button" target="_blank" rel="noreferrer noopener">
  <img src="https://nowpayments.io/images/embeds/payments-button-white.svg" alt="Cryptocurrency & Bitcoin payment button by NOWPayments" />
</a></div>
  <div>
      <label htmlFor="username-input" style={{ fontWeight: 'bold', color: '#555'  }}>
        Username:
      </label>
      
      <input
        id="username-input"
        type="text"
        value={username}
        onChange={handleChange}
        placeholder="Enter your username"
        style={{ padding: '8px', borderRadius: '4px', border: '1px solid #ccc', backgroundColor: 'black', color: 'white'}}
      />
      
      <p style={{ fontSize: '14px', color: '#555' }}>
        Current state: <strong>{username}</strong>
      </p>
    </div>
</div>  

     </section>
    </main>
  );
}
