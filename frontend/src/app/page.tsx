'use client';

import { FormEvent, useState, ChangeEvent, useRef } from 'react';
import styles from './page.module.css';

type ChatMessage = {
    role: string;
    content: string;
    image?: string | null; // Added to render user images in the chat log
    x: number | null;
    y: number | null;
    z: number | null;
};

export default function Home() {
    const [input, setInput] = useState('');
    const [loading, setLoading] = useState(false);
    const [username, setUsername] = useState<string>('');
    const [selectedImage, setSelectedImage] = useState<string | null>(null); // Base64 string state
    const fileInputRef = useRef<HTMLInputElement>(null);

    const [messages, setMessages] = useState<ChatMessage[]>([
        {
            role: 'assistant',
            content: 'Hello! I am FairAI. Ask me about fairness, trust, or AI governance.',
            x: null,
            y: null,
            z: null
        },
    ]);

    const handleChange = (event: ChangeEvent<HTMLInputElement>): void => {
        setUsername(event.target.value);
    };

    // Handle image file picking and convert it to base64
    const handleImageChange = (event: ChangeEvent<HTMLInputElement>): void => {
        const file = event.target.files?.[0];
        if (!file) return;

        const reader = new FileReader();
        reader.onloadend = () => {
            setSelectedImage(reader.result as string); // Stores data:image/...;base64,...
        };
        reader.readAsDataURL(file);
    };

    async function handleSubmit(event: FormEvent) {
        event.preventDefault();
        const trimmed = input.trim();
        // Allow sending if there is text OR an image
        if ((!trimmed && !selectedImage) || loading) return;

        const apiBaseUrl =
            process.env.NEXT_PUBLIC_API_URL ||
            (typeof window !== 'undefined' && window.location.hostname.endsWith('.app.github.dev')
                ? `https://${window.location.hostname.replace('-3000.', '-5124.')}`
                : typeof window !== 'undefined' ? window.location.origin : 'http://localhost:5124');

        // Append locally to user chat list immediately (including the preview string if present)
        setMessages((current) => [
            ...current,
            { role: 'user', content: trimmed, image: selectedImage, x: null, y: null, z: null }
        ]);

        // Cache current image data and reset inputs right away
        const imageToSend = selectedImage;
        setInput('');
        setSelectedImage(null);
        if (fileInputRef.current) fileInputRef.current.value = '';
        setLoading(true);

        try {
            const response = await fetch(`${apiBaseUrl}/api/chat`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({
                    message: trimmed + ' ' + imageToSend,
                    username: username,
                }),
            });

            if (!response.ok) {
                throw new Error('Request failed');
            }

            const data = await response.json();

            // --- PARSING EXTRACTION FOR BASE64 IMAGES ---
            let textContent = data.message || '';
            let extractedImage: string | null = null;

            // Split string into separate chunks by blanks/whitespace
            const words = textContent.split(/\s+/);

            // Find an item matching the base64 media marker
            const base64ImageWord = words.find((word: string) => word.startsWith('data:image/'));

            if (base64ImageWord) {
                extractedImage = base64ImageWord;
                // Replace the massive blob parameter inside the string payload
                textContent = textContent.replace(base64ImageWord, '[Image displayed below]').trim();
            }

            setMessages((current) => [
                ...current,
                {
                    role: 'assistant',
                    content: textContent,
                    image: extractedImage,
                    x: data.x,
                    y: data.y,
                    z: data.z
                }
            ]);
        }
        catch (error) {
            setMessages((current) => [
                ...current,
                { role: 'assistant', content: 'FairAI is unavailable right now. Please make sure the API is running.', x: null, y: null, z: null },
            ]);
        }
        finally {
            setLoading(false);
        }
    }    const handleVote = async (index: number, x: number | null, y: number | null, z: number | null, voteType: 'up' | 'down') => {
        if (x === null || y === null || z === null) return;

        try {
            const response = await fetch(`/api/vote`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ x, y, z, voteType }),
            });

            if (!response.ok) throw new Error('Vote failed');
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
                            FairAI Chat
                        </h1>
                    </div>
                    <span className={styles.status}>Live</span>
                </header>

                <div className={styles.chatWindow}>
                    {messages.map((message, index) => (
                        <div key={`${message.role}-${index}`} className={`${styles.messageRow} ${styles[message.role]}`}>
                            <div className={styles.avatar}>{message.role === 'assistant' ? 'FA' : 'YO'}</div>
                            <div className={styles.bubble}>
                                {message.image && (
                                    <div style={{ marginBottom: '8px' }}>
                                        <img
                                            src={message.image}
                                            alt="User attachment"
                                            style={{ maxWidth: '200px', borderRadius: '8px', display: 'block' }}
                                        />
                                    </div>
                                )}
                                {message.content}
                            </div>

                            {message.role === 'assistant' && message.x !== null && (
                                <div style={{ display: 'flex', gap: '8px', marginTop: '4px' }}>
                                    <button
                                        onClick={() => handleVote(index, message.x, message.y, message.z, 'up')}
                                        className="px-3 py-1 bg-green-500 text-white rounded text-sm hover:bg-green-600"
                                    >
                                        Vote ++
                                    </button>
                                    <button
                                        onClick={() => handleVote(index, message.x, message.y, message.z, 'down')}
                                        className="px-3 py-1 bg-red-500 text-white rounded text-sm hover:bg-red-600"
                                    >
                                        Vote --
                                    </button>
                                </div>
                            )}
                        </div>
                    ))}
                    {loading && (
                        <div className={`${styles.messageRow} ${styles.assistant}`}>
                            <div className={styles.avatar}>FA</div>
                            <div className={styles.bubble}>Thinking…</div>
                        </div>
                    )}
                </div>

                {/* Form changes: File preview UI and File input trigger */}
                <form className={styles.form} onSubmit={handleSubmit}>
                    {selectedImage && (
                        <div style={{ position: 'absolute', bottom: '60px', left: '20px', background: '#222', padding: '5px', borderRadius: '6px', border: '1px solid #444', display: 'flex', alignItems: 'center', gap: '8px' }}>
                            <img src={selectedImage} alt="Preview" style={{ width: '40px', height: '40px', objectFit: 'cover', borderRadius: '4px' }} />
                            <button type="button" onClick={() => { setSelectedImage(null); if (fileInputRef.current) fileInputRef.current.value = ''; }} style={{ background: 'transparent', color: 'red', border: 'none', cursor: 'pointer', fontWeight: 'bold' }}>✕</button>
                        </div>
                    )}

                    <input
                        type="file"
                        accept="image/*"
                        ref={fileInputRef}
                        onChange={handleImageChange}
                        style={{ display: 'none' }}
                        id="chat-image-upload"
                    />

                    <button
                        type="button"
                        onClick={() => fileInputRef.current?.click()}
                        style={{ padding: '0 12px', background: '#333', color: '#fff', border: 'none', cursor: 'pointer', fontSize: '18px' }}
                        title="Upload Image"
                    >
                        📷
                    </button>

                    <input
                        value={input}
                        onChange={(event) => setInput(event.target.value)}
                        placeholder="Type your message..."
                        aria-label="Chat input"
                    />
                    <button type="submit" disabled={loading || (!input.trim() && !selectedImage)}>
                        {loading ? 'Sending...' : 'Send'}
                    </button>
                </form>

                <div style={{ display: 'flex', flexWrap: 'wrap', marginTop: '20px', gap: '15px' }}>
                    <div style={{ backgroundColor: 'lightcoral', padding: '5px', borderRadius: '4px' }}>
                        <a href="https://nowpayments.io/payment/?iid=5715057547&source=button" target="_blank" rel="noreferrer noopener">
                            <img src="https://nowpayments.io/images/embeds/payments-button-black.svg" alt="Crypto payment button by NOWPayments" />
                        </a>
                    </div>
                    <div style={{ backgroundColor: 'lightgreen', padding: '5px', borderRadius: '4px' }}>
                        <a href="https://nowpayments.io/payment/?iid=4914149117&source=button" target="_blank" rel="noreferrer noopener">
                            <img src="https://nowpayments.io/images/embeds/payments-button-white.svg" alt="Cryptocurrency & Bitcoin payment button by NOWPayments" />
                        </a>
                    </div>
                    <div>
                        <label htmlFor="username-input" style={{ fontWeight: 'bold', color: '#555', marginRight: '8px' }}>
                            Username:
                        </label>
                        <input
                            id="username-input"
                            type="text"
                            value={username}
                            onChange={handleChange}
                            placeholder="Enter your username"
                            style={{ padding: '8px', borderRadius: '4px', border: '1px solid #ccc', backgroundColor: 'black', color: 'white' }}
                        />
                        <p style={{ fontSize: '14px', color: '#555', marginTop: '4px' }}>
                            Current state: <strong>{username}</strong>
                        </p>
                    </div>
                </div>

            </section>
        </main>
    );
}
