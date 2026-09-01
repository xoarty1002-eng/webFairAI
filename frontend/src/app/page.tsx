'use client';

import { FormEvent, useState } from 'react';
import styles from './page.module.css';

type ChatMessage = {
  role: string;
  content: string;
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

  async function handleSubmit(event: FormEvent) {
    event.preventDefault();
    const trimmed = input.trim();
    if (!trimmed || loading) return;

    const apiBaseUrl = process.env.NEXT_PUBLIC_API_URL ?? 'http://localhost:5124';

    setMessages((current) => [...current, { role: 'user', content: trimmed }]);
    setInput('');
    setLoading(true);

    try {
      const response = await fetch(`${apiBaseUrl}/api/chat`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ message: trimmed }),
      });

      if (!response.ok) {
        throw new Error('Request failed');
      }

      const data = await response.json();
      setMessages((current) => [...current, { role: 'assistant', content: data.message }]);
    } catch (error) {
      setMessages((current) => [
        ...current,
        { role: 'assistant', content: 'FairAI is unavailable right now. Please make sure the API is running.' },
      ]);
    } finally {
      setLoading(false);
    }
  }

  return (
    <main className={styles.page}>
      <section className={styles.shell}>
        <header className={styles.header}>
          <div>
            <p className={styles.eyebrow}>Governance AI</p>
            <h1>FairAI Chat</h1>
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
      </section>
    </main>
  );
}
