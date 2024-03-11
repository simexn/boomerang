import {getToken} from './authHandler';


export interface Message{
    id: number;
    message: string;
    chatId: number;
    fromUser: number;
    date: string;
}

export async function fetchMessages(chatId: string){
    let token = await getToken();
    
    const response = await fetch(`https://localhost:5000/chat/getMessages?chatId=${chatId}`, {
        method: 'GET',
        headers: {
            'Authorization': `Bearer ${token}`,
            'Content-Type': 'application/json'
        },
        credentials: 'include'
    });
    
    const data = await response.json();
    
    
    if (response.ok) {
        if (data.messages) {
            const messages: Message[] = await data.messages.map((message: any) => ({
                id: message.id,
                message: message.text,
                chatId: message.chatId,
                fromUser: message.fromUser.userName,
                date: new Date(message.timestamp).toLocaleString()
            }));
    
            console.log(messages);
            return messages;
        } else {
            
            return [];
        }
        
    } else {
        throw new Error(data);
    }
};


export async function handleMessageSubmit(messageToSubmit: string, chatId: string, roomId: string){
    let token = await getToken();

    const array = [messageToSubmit, chatId, roomId]
    

    const requestBody = JSON.stringify(array);

    const response = await fetch('https://localhost:5000/chat/sendMessage', {
        method: 'POST',
        headers: {
            'Authorization': `Bearer ${token}`,
            'Content-Type': 'application/json'
        },
        credentials: 'include',
        body: requestBody
    });

    if (!response.ok) {
        console.error('Error sending message:', await response.text());
    }
}








