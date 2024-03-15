import {getToken} from './authHandler';
const backendUrl = 'http://simexn-001-site1.ktempurl.com'

export interface Message{
    id: number;
    message: string;
    chatId: number;
    fromUser: number;
    fromUserId: number;
    date: string;
}

export async function fetchMessages(chatId: string){
    let token = await getToken();
    
    const response = await fetch(`${backendUrl}/chat/getMessages?chatId=${chatId}`, {
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
                fromUserId: message.fromUser.id,
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

    const response = await fetch(`${backendUrl}/chat/sendMessage`, {
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








