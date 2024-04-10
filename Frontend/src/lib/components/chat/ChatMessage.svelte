<script lang="ts">
    import type { User } from "$lib/Handlers/accountHandler";
    import type {ChatItem} from "$lib/Handlers/chatHandler";
    import { writable, type Stores } from "svelte/store";
    import { handleEditMessage, handleDeleteMessage } from "$lib/Handlers/chatHandler";
    import EventMessage from "./EventMessage.svelte";
    import UserMessage from "./UserMessage.svelte";
    import '$lib/css/message.css';
    import { onMount } from "svelte";
    import type { Group } from "$lib/Handlers/groupHandler";
    export let imageUrl: string;
    export let userInfo: User;
    export let scrollContainer: HTMLElement;
    export let chatItems:ChatItem[];
    export let chatId: string;
    export let groupInfo: Group;
    const isEditing = writable();

    let originalMessage = '';

    let previousTimestamp:any = null;

    onMount(() => {
        console.log("rendering Chat Message component");
        //scrollContainer.scrollTop = scrollContainer.scrollHeight;
    });

    function isEditingMessage(id: number, content:string) {
        originalMessage = content;
        isEditing.set(id);
    }

    async function confirmEdit(id:number, newContent: string) {
        
        await handleEditMessage(chatId, id, newContent);
        isEditing.set(null);
    }

    async function cancelEdit(id: number) {
        isEditing.set(null);
        chatItems.map(item => item.id === id ? item.content = originalMessage : item);
    }
    
    async function deleteMessage(id: number) {
        await handleDeleteMessage(chatId, id);
    }

    
    onMount(() => {
        console.log("rendering Chat Message component")
    });
    
</script>

<div class="messages-container" bind:this={scrollContainer}>
    
    {#each chatItems as item (item.id)}
    <div class="message" style="display: flex; align-items: start;">
        {#if item.isEvent}
            <EventMessage {item} />
        {:else}
            <UserMessage {groupInfo} {item} {imageUrl} {userInfo} {isEditingMessage} {deleteMessage} {confirmEdit} {cancelEdit} {isEditing} withoutUserDetails={false} />
        {/if}
    </div>
    {/each}
</div>

<style>
.messages-container{
    display: flex;
    flex-direction: column;
    word-wrap: break-word;
    padding: 0 0 0 5px;
    height: 100% !important;
    min-height: 0 !important;
    max-height: 100% !important;
    box-sizing: border-box;
    overflow: auto;
}
.message {
    transition: background-color 100ms;
    position:relative;
    padding-left: 1rem;
}

.message:hover {
    background-color: #f5f5f5;
}


</style>